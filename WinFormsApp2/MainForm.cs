using BankSystem.Shared.DTOs;
using BankSystem.Shared.Entities;
using System.Net.Http.Json;

namespace BankSystem.TellerApp
{
    /// <summary>
    /// Теллерийн үндсэн форм. Нэвтэрсний дараа нээгдэнэ.
    /// Гурван таб: дугаар дуудах (+ жагсаалт), гүйлгээ (+ үлдэгдэл), ханш.
    /// </summary>
    public class MainForm : Form
    {
        private readonly HttpClient _http;
        private readonly string _serverUrl;
        private readonly string _token;
        private readonly int _windowId;
        private TabControl _tabs = null!;

        public MainForm(string token, int windowId, HttpClient http, string serverUrl)
        {
            _token = token;
            _windowId = windowId;
            _http = http;
            _serverUrl = serverUrl;
            SetupUI();
        }

        private void SetupUI()
        {
            Text = $"Теллерийн апп — Цонх {_windowId}";
            Size = new Size(720, 580);
            StartPosition = FormStartPosition.CenterScreen;

            _tabs = new TabControl { Dock = DockStyle.Fill };
            _tabs.TabPages.Add(CreateQueueTab());
            _tabs.TabPages.Add(CreateTransferTab());
            _tabs.TabPages.Add(CreateRateTab());
            Controls.Add(_tabs);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Форм нээгдэхэд дарааллын жагсаалтыг татна
            _ = RefreshQueueListAsync();
        }

        // ── Таб 1: Дугаар дуудах + жагсаалт ────────────────────────────

        private TabPage CreateQueueTab()
        {
            var tab = new TabPage("🔢 Дугаар дуудах");

            // ── Зүүн тал: удирдлага ──
            var lblCurrent = new Label
            {
                Name = "lblCurrentNumber",
                Text = "Одоогийн дугаар: —",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(20, 20),
                Size = new Size(390, 45)
            };

            var lblQueue = new Label
            {
                Name = "lblQueueCount",
                Text = "Хүлээж байна: — хүн",
                Font = new Font("Segoe UI", 13),
                Location = new Point(20, 72),
                Size = new Size(320, 30)
            };

            var btnCallNext = new Button
            {
                Text = "▶ ДАРААГИЙН ҮЙЛЧЛҮҮЛЭГЧ",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 153, 51),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, 115),
                Size = new Size(340, 55),
                Cursor = Cursors.Hand
            };
            btnCallNext.Click += BtnCallNext_Click;

            var btnRefresh = new Button
            {
                Text = "🔄 Шинэчлэх",
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 185),
                Size = new Size(150, 36),
                Cursor = Cursors.Hand
            };
            btnRefresh.Click += async (s, e) =>
            {
                await RefreshQueueStatus(tab);
                await RefreshQueueListAsync();
            };

            // ── Баруун тал: хүлээлтийн жагсаалт ──
            var lblListTitle = new Label
            {
                Text = "Хүлээж буй дугаар:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(430, 20),
                Size = new Size(240, 25)
            };

            var lstQueue = new ListBox
            {
                Name = "lstQueue",
                Font = new Font("Segoe UI", 13),
                Location = new Point(430, 50),
                Size = new Size(240, 390),
                BorderStyle = BorderStyle.FixedSingle
            };

            tab.Controls.AddRange(new Control[]
            {
                lblCurrent, lblQueue, btnCallNext, btnRefresh,
                lblListTitle, lstQueue
            });
            return tab;
        }

        private async void BtnCallNext_Click(object? sender, EventArgs e)
        {
            var btn = (Button)sender!;
            btn.Enabled = false;
            try
            {
                var response = await _http.PostAsJsonAsync(
                    $"{_serverUrl}/api/ticket/call-next", _windowId);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CallNextResponse>();
                    var tab = _tabs.TabPages[0];
                    ((Label)tab.Controls["lblCurrentNumber"]!).Text =
                        $"Одоогийн дугаар: {result?.TicketNumber:D3}";
                    ((Label)tab.Controls["lblQueueCount"]!).Text =
                        $"Хүлээж байна: {result?.RemainingCount} хүн";
                    // Дуудсаны дараа жагсаалтыг шинэчилнэ
                    await RefreshQueueListAsync();
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(content.Trim('"'), "Мэдэгдэл",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Алдаа: {ex.Message}"); }
            finally { btn.Enabled = true; }
        }

        private async Task RefreshQueueStatus(TabPage tab)
        {
            try
            {
                var res = await _http.GetAsync($"{_serverUrl}/api/ticket/status");
                if (!res.IsSuccessStatusCode) return;
                var result = await res.Content.ReadFromJsonAsync<StatusResponse>();
                ((Label)tab.Controls["lblCurrentNumber"]!).Text =
                    $"Одоогийн дугаар: {result?.CurrentNumber:D3}";
                ((Label)tab.Controls["lblQueueCount"]!).Text =
                    $"Хүлээж байна: {result?.QueueCount} хүн";
            }
            catch { }
        }

        /// <summary>
        /// GET /api/ticket/queue → хүлээж буй дугааруудын жагсаалтыг авч ListBox-т харуулна.
        /// Дугаар дуудах болон шинэчлэх товч дарахад дуудагдана.
        /// </summary>
        private async Task RefreshQueueListAsync()
        {
            try
            {
                var res = await _http.GetAsync($"{_serverUrl}/api/ticket/queue");
                if (!res.IsSuccessStatusCode) return;
                var numbers = await res.Content.ReadFromJsonAsync<List<int>>();
                if (numbers is null) return;

                var lst = (ListBox)_tabs.TabPages[0].Controls["lstQueue"]!;
                lst.BeginUpdate();
                lst.Items.Clear();
                foreach (var n in numbers)
                    lst.Items.Add($"№ {n:D3}");
                lst.EndUpdate();
            }
            catch { }
        }

        // ── Таб 2: Гүйлгээ + үлдэгдэл шалгах ───────────────────────────

        private TabPage CreateTransferTab()
        {
            var tab = new TabPage("💳 Гүйлгээ");

            // ── Гүйлгээний хэсэг ──
            tab.Controls.Add(MakeLabel("Илгээгч данс:", 20, 25));
            tab.Controls.Add(MakeTextBox("txtFrom", 190, 20));
            tab.Controls.Add(MakeLabel("Хүлээн авагч:", 20, 67));
            tab.Controls.Add(MakeTextBox("txtTo", 190, 62));
            tab.Controls.Add(MakeLabel("Дүн (₮):", 20, 109));
            tab.Controls.Add(MakeTextBox("txtAmount", 190, 104));

            var btnTransfer = new Button
            {
                Text = "💸 ГҮЙЛГЭЭ ХИЙХ",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, 148),
                Size = new Size(280, 48),
                Cursor = Cursors.Hand
            };
            btnTransfer.Click += BtnTransfer_Click;
            tab.Controls.Add(btnTransfer);

            tab.Controls.Add(new Label
            {
                Name = "lblTransferResult",
                Location = new Point(20, 210),
                Size = new Size(550, 28),
                Font = new Font("Segoe UI", 11)
            });

            // ── Хэвтээ хуваагч шугам ──
            tab.Controls.Add(new Label
            {
                Text = "─────────── Данс шалгах ───────────",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(20, 250),
                Size = new Size(450, 22),
                TextAlign = ContentAlignment.MiddleCenter
            });

            // ── Үлдэгдэл шалгах хэсэг ──
            tab.Controls.Add(MakeLabel("Дансны дугаар:", 20, 282));
            tab.Controls.Add(MakeTextBox("txtCheckAccount", 190, 277));

            var btnCheck = new Button
            {
                Text = "🔍 Шалгах",
                Font = new Font("Segoe UI", 11),
                Location = new Point(400, 277),
                Size = new Size(110, 30),
                Cursor = Cursors.Hand
            };
            btnCheck.Click += BtnCheckBalance_Click;
            tab.Controls.Add(btnCheck);

            tab.Controls.Add(new Label
            {
                Name = "lblBalanceResult",
                Location = new Point(20, 322),
                Size = new Size(600, 80),
                Font = new Font("Segoe UI", 11)
            });

            return tab;
        }

        private async void BtnTransfer_Click(object? sender, EventArgs e)
        {
            var tab = _tabs.TabPages[1];
            var from = ((TextBox)tab.Controls["txtFrom"]!).Text.Trim();
            var to = ((TextBox)tab.Controls["txtTo"]!).Text.Trim();
            var amtText = ((TextBox)tab.Controls["txtAmount"]!).Text.Trim();
            var lbl = (Label)tab.Controls["lblTransferResult"]!;

            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
            { MessageBox.Show("Дансны дугаар оруулна уу!"); return; }

            if (!decimal.TryParse(amtText, out decimal amount) || amount <= 0)
            { MessageBox.Show("Зөв дүн оруулна уу!"); return; }

            try
            {
                var res = await _http.PostAsJsonAsync(
                    $"{_serverUrl}/api/account/transfer",
                    new TransferRequest
                    {
                        FromAccountNumber = from,
                        ToAccountNumber = to,
                        Amount = amount,
                        TellerWindowId = _windowId
                    });

                var result = await res.Content.ReadFromJsonAsync<TransferResponse>();
                lbl.Text = result?.Success == true
                    ? $"✅ {amount:N0}₮ амжилттай шилжүүллээ"
                    : $"❌ {result?.Message}";
                lbl.ForeColor = result?.Success == true ? Color.Green : Color.Red;
            }
            catch (Exception ex)
            {
                lbl.Text = $"Алдаа: {ex.Message}";
                lbl.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// GET /api/account/{accountNumber} → данс эзэмшигчийн нэр болон үлдэгдэл харуулна.
        /// BankAccount entity нь Shared DLL-д байгаа тул шууд ашиглана.
        /// </summary>
        private async void BtnCheckBalance_Click(object? sender, EventArgs e)
        {
            var tab = _tabs.TabPages[1];
            var accountNumber = ((TextBox)tab.Controls["txtCheckAccount"]!).Text.Trim();
            var lbl = (Label)tab.Controls["lblBalanceResult"]!;

            if (string.IsNullOrEmpty(accountNumber))
            { MessageBox.Show("Дансны дугаар оруулна уу!"); return; }

            var btn = (Button)sender!;
            btn.Enabled = false;
            try
            {
                var res = await _http.GetAsync($"{_serverUrl}/api/account/{accountNumber}");
                if (res.IsSuccessStatusCode)
                {
                    var account = await res.Content.ReadFromJsonAsync<BankAccount>();
                    if (account is not null)
                    {
                        lbl.Text = $"👤 {account.OwnerName}\n💰 Үлдэгдэл: {account.Balance:N0} ₮";
                        lbl.ForeColor = Color.FromArgb(0, 80, 0);
                    }
                }
                else
                {
                    lbl.Text = "❌ Данс олдсонгүй";
                    lbl.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lbl.Text = $"Алдаа: {ex.Message}";
                lbl.ForeColor = Color.Red;
            }
            finally { btn.Enabled = true; }
        }

        // ── Таб 3: Ханш өөрчлөх ──────────────────────────────────────────

        private TabPage CreateRateTab()
        {
            var tab = new TabPage("💱 Ханш");

            tab.Controls.Add(MakeLabel("Валют:", 20, 25));
            var cmb = new ComboBox
            {
                Name = "cmbCurrency",
                Location = new Point(150, 20),
                Size = new Size(130, 28),
                Font = new Font("Segoe UI", 11),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmb.Items.AddRange(new[] { "USD", "EUR", "CNY", "RUB" });
            cmb.SelectedIndex = 0;
            tab.Controls.Add(cmb);

            tab.Controls.Add(MakeLabel("Авах ханш:", 20, 72));
            tab.Controls.Add(MakeTextBox("txtBuy", 150, 67));
            tab.Controls.Add(MakeLabel("Зарах ханш:", 20, 119));
            tab.Controls.Add(MakeTextBox("txtSell", 150, 114));

            var btnUpdate = new Button
            {
                Text = "📊 ХАНШ ШИНЭЧЛЭХ",
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                BackColor = Color.FromArgb(204, 102, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, 160),
                Size = new Size(280, 50),
                Cursor = Cursors.Hand
            };
            btnUpdate.Click += BtnUpdateRate_Click;
            tab.Controls.Add(btnUpdate);

            tab.Controls.Add(new Label
            {
                Name = "lblRateResult",
                Location = new Point(20, 225),
                Size = new Size(500, 30),
                Font = new Font("Segoe UI", 11)
            });

            return tab;
        }

        private async void BtnUpdateRate_Click(object? sender, EventArgs e)
        {
            var tab = _tabs.TabPages[2];
            var currency = ((ComboBox)tab.Controls["cmbCurrency"]!).SelectedItem!.ToString()!;
            var lbl = (Label)tab.Controls["lblRateResult"]!;

            if (!decimal.TryParse(((TextBox)tab.Controls["txtBuy"]!).Text, out decimal buy) ||
                !decimal.TryParse(((TextBox)tab.Controls["txtSell"]!).Text, out decimal sell))
            { MessageBox.Show("Зөв ханш оруулна уу!"); return; }

            try
            {
                var res = await _http.PutAsJsonAsync(
                    $"{_serverUrl}/api/exchangerate/{currency}",
                    new UpdateRateRequest
                    {
                        BuyRate = buy,
                        SellRate = sell,
                        TellerWindowId = _windowId
                    });

                lbl.Text = res.IsSuccessStatusCode
                    ? $"✅ {currency}: авах {buy:N0}, зарах {sell:N0} — шинэчлэгдлээ"
                    : "❌ Алдаа гарлаа";
                lbl.ForeColor = res.IsSuccessStatusCode ? Color.Green : Color.Red;
            }
            catch (Exception ex)
            {
                lbl.Text = $"Алдаа: {ex.Message}";
                lbl.ForeColor = Color.Red;
            }
        }

        // ── Туслах методууд ───────────────────────────────────────────────

        private static Label MakeLabel(string text, int x, int y) => new()
        {
            Text = text, Location = new Point(x, y),
            Size = new Size(165, 25), Font = new Font("Segoe UI", 11)
        };

        private static TextBox MakeTextBox(string name, int x, int y) => new()
        {
            Name = name, Location = new Point(x, y),
            Size = new Size(200, 28), Font = new Font("Segoe UI", 11)
        };

        /// <summary>GET /api/ticket/status-д хэрэглэх хариу загвар.</summary>
        private record StatusResponse(int CurrentNumber, int QueueCount);
    }
}
