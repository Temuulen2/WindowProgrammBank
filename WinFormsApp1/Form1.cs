using BankSystem.Shared.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace BankSystem.NumberTerminal
{
    /// <summary>
    /// Банкны үүдэнд байрлах дугаар авах терминал.
    /// Харилцагч үйлчилгээ сонгоод товч дарахад дугаар авна.
    /// TCP Socket-ээр серверт холбогдож дуудагдсан дугаарыг realtime харуулна.
    /// </summary>
    public partial class TicketTerminalForm : Form
    {
        private static readonly HttpClient _http = new();
        private const string ServerHttp = "http://localhost:5272";
        private const string SocketHost = "localhost";
        private const int SocketPort = 9001;

        /// <summary>TCP холболт — нэг удаа холбогдоод байнга сонсоно.</summary>
        private TcpClient? _tcpClient;
        private CancellationTokenSource _cts = new();

        public TicketTerminalForm()
        {
            InitializeComponent();
            SetupUI();
            _ = ConnectSocketAsync();
        }

        private void SetupUI()
        {
            Text = "Дугаар авах терминал";
            Size = new Size(500, 440);
            BackColor = Color.FromArgb(240, 248, 255);
            StartPosition = FormStartPosition.CenterScreen;

            var lblTitle = new Label
            {
                Text = "🏦 ХААН БАНК",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(50, 20),
                Size = new Size(400, 50)
            };

            // Олгосон дугаарыг том font-оор харуулна
            var lblIssuedNumber = new Label
            {
                Name = "lblIssuedNumber",
                Text = "—",
                Font = new Font("Segoe UI", 72, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(50, 75),
                Size = new Size(400, 120)
            };

            var lblQueue = new Label
            {
                Name = "lblQueue",
                Text = "Хүлээлт: —",
                Font = new Font("Segoe UI", 13),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(100, 200),
                Size = new Size(300, 30)
            };

            var cmbService = new ComboBox
            {
                Name = "cmbService",
                Font = new Font("Segoe UI", 12),
                Location = new Point(150, 240),
                Size = new Size(200, 35),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbService.Items.AddRange(new[] { "Гүйлгээ", "Лавлагаа", "Зээл", "Карт" });
            cmbService.SelectedIndex = 0;

            var btnIssue = new Button
            {
                Name = "btnIssue",
                Text = "🎫 ДУГААР АВАХ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 153, 51),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(100, 290),
                Size = new Size(300, 60),
                Cursor = Cursors.Hand
            };
            btnIssue.Click += BtnIssue_Click;

            var lblStatus = new Label
            {
                Name = "lblStatus",
                Text = "Сервертэй холбогдож байна...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Gray,
                Location = new Point(50, 365),
                Size = new Size(400, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.AddRange(new Control[]
            {
                lblTitle, lblIssuedNumber, lblQueue, cmbService, btnIssue, lblStatus
            });
        }

        /// <summary>
        /// Дугаар авах товч. Сервераас дараагийн дугаар авч харуулна.
        /// async void: UI event handler-д зөвшөөрөгдөнө.
        /// </summary>
        private async void BtnIssue_Click(object? sender, EventArgs e)
        {
            var btn = (Button)Controls["btnIssue"]!;
            btn.Enabled = false;
            btn.Text = "Уншиж байна...";
            try
            {
                var response = await _http.PostAsync($"{ServerHttp}/api/ticket/issue", null);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<IssueTicketResponse>();
                    if (result is not null)
                    {
                        ((Label)Controls["lblIssuedNumber"]!).Text = result.TicketNumber.ToString("D3");
                        ((Label)Controls["lblQueue"]!).Text = $"Хүлээж байгаа: {result.QueueCount} хүн";
                        ShowTicketDialog(result);
                    }
                }
                else
                {
                    MessageBox.Show("Серверт холбогдоход алдаа гарлаа!", "Алдаа",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Сервер ажиллаж байгаа эсэхийг шалгана уу.",
                    "Холболтын алдаа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                btn.Enabled = true;
                btn.Text = "🎫 ДУГААР АВАХ";
            }
        }

        /// <summary>
        /// Тасалбарын мэдээллийг харуулна.
        /// Жинхэнэ системд ESC/POS printer protocol ашиглан хэвлэнэ.
        /// </summary>
        private void ShowTicketDialog(IssueTicketResponse result)
        {
            MessageBox.Show(
                $"Таны дугаар: {result.TicketNumber:D3}\nЦаг: {result.IssuedAt:HH:mm}\nХүлээлт: {result.QueueCount} хүн",
                "Дугаар амжилттай авлаа!",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// TCP Socket-ээр серверт холбогдоно — нэг удаа холбогдоод байнга сонсоно.
        /// Сервер SHOW_NUMBER мессеж явуулахад дэлгэцийг шинэчилнэ.
        /// </summary>
        private async Task ConnectSocketAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    _tcpClient = new TcpClient();
                    await _tcpClient.ConnectAsync(SocketHost, SocketPort, _cts.Token);
                    SetStatus("● Холбогдсон", Color.Green);

                    await ListenSocketAsync(_tcpClient, _cts.Token);
                }
                catch (OperationCanceledException) { break; }
                catch
                {
                    SetStatus("● Холбогдоогүй — дахин оролдож байна...", Color.OrangeRed);
                    await Task.Delay(3000, _cts.Token).ContinueWith(_ => { });
                }
            }
        }

        /// <summary>
        /// TCP stream-ийг мөр мөрөөр уншиж SHOW_NUMBER мессежийг боловсруулна.
        /// </summary>
        private async Task ListenSocketAsync(TcpClient client, CancellationToken ct)
        {
            using var reader = new System.IO.StreamReader(client.GetStream(), Encoding.UTF8);
            while (!ct.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync(ct);
                if (line is null) break;

                try
                {
                    var msg = JsonSerializer.Deserialize<SocketMessageDto>(line,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (msg?.Type == "SHOW_NUMBER" && msg.Payload is not null)
                    {
                        var data = JsonSerializer.Deserialize<CallNextResponse>(msg.Payload,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (data is not null)
                        {
                            // UI thread-д шинэчилнэ
                            Invoke(() =>
                            {
                                ((Label)Controls["lblIssuedNumber"]!).Text =
                                    data.TicketNumber.ToString("D3");
                                ((Label)Controls["lblQueue"]!).Text =
                                    $"Хүлээж байгаа: {data.RemainingCount} хүн";
                            });
                        }
                    }
                }
                catch { /* JSON parse алдаа алгасна */ }
            }
        }

        private void SetStatus(string text, Color color)
        {
            if (InvokeRequired)
                Invoke(() => SetStatus(text, color));
            else
            {
                var lbl = (Label)Controls["lblStatus"]!;
                lbl.Text = text;
                lbl.ForeColor = color;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _cts.Cancel();
            _tcpClient?.Dispose();
            base.OnFormClosed(e);
        }

        /// <summary>SocketMessage JSON deserialize-д ашиглах локал DTO.</summary>
        private record SocketMessageDto(string Type, string? Payload);
    }
}
