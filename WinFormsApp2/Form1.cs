using BankSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace BankSystem.TellerApp
{
    /// <summary>
    /// Теллерийн нэвтрэх форм.
    /// Амжилттай нэвтэрсэний дараа MainForm нээгдэж энэ форм хаагдана.
    /// </summary>
    public partial class LoginForm : Form
    {
        private static readonly HttpClient _http = new();
        private const string ServerUrl = "http://localhost:5272";

        public LoginForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            Text = "Теллерийн нэвтрэлт";
            Size = new Size(400, 320);
            BackColor = Color.FromArgb(240, 248, 255);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = "🏦 ХААН БАНК",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(0, 102, 204),
                Location = new Point(50, 20),
                Size = new Size(300, 45)
            };

            var lblSub = new Label
            {
                Text = "Теллерийн систем",
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Location = new Point(50, 60),
                Size = new Size(300, 25)
            };

            var lblUser = new Label
            {
                Text = "Нэвтрэх нэр:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(60, 105),
                Size = new Size(120, 25)
            };

            var txtUsername = new TextBox
            {
                Name = "txtUsername",
                Font = new Font("Segoe UI", 11),
                Location = new Point(185, 102),
                Size = new Size(155, 28)
            };

            var lblPass = new Label
            {
                Text = "Нууц үг:",
                Font = new Font("Segoe UI", 11),
                Location = new Point(60, 148),
                Size = new Size(120, 25)
            };

            var txtPassword = new TextBox
            {
                Name = "txtPassword",
                Font = new Font("Segoe UI", 11),
                Location = new Point(185, 145),
                Size = new Size(155, 28),
                PasswordChar = '●'
            };

            var btnLogin = new Button
            {
                Name = "btnLogin",
                Text = "НЭВТРЭХ",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(100, 195),
                Size = new Size(200, 45),
                Cursor = Cursors.Hand
            };
            btnLogin.Click += BtnLogin_Click;

            // Enter дарахад нэвтрэх
            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) btnLogin.PerformClick();
            };

            var lblError = new Label
            {
                Name = "lblError",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Red,
                Location = new Point(60, 250),
                Size = new Size(280, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.AddRange(new Control[]
            {
                lblTitle, lblSub, lblUser, txtUsername,
                lblPass, txtPassword, btnLogin, lblError
            });
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            var btn = (Button)Controls["btnLogin"]!;
            var username = ((TextBox)Controls["txtUsername"]!).Text.Trim();
            var password = ((TextBox)Controls["txtPassword"]!).Text;
            var lblError = (Label)Controls["lblError"]!;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Нэвтрэх нэр болон нууц үг оруулна уу";
                return;
            }

            btn.Enabled = false;
            btn.Text = "Нэвтэрч байна...";
            lblError.Text = "";

            try
            {
                var response = await _http.PostAsJsonAsync(
                    $"{ServerUrl}/api/auth/login",
                    new LoginRequest { Username = username, Password = password });

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result?.Success == true && result.Token is not null)
                {
                    // Амжилттай нэвтэрсэн: MainForm нээж энэ форм хаана
                    var mainForm = new MainForm(result.Token, result.TellerWindowId ?? 1, _http, ServerUrl);
                    mainForm.Show();
                    Hide();
                    mainForm.FormClosed += (s, e) => Close();
                }
                else
                {
                    lblError.Text = result?.Message ?? "Нэвтрэх нэр эсвэл нууц үг буруу";
                }
            }
            catch
            {
                lblError.Text = "Сервертэй холбогдоход алдаа гарлаа";
            }
            finally
            {
                btn.Enabled = true;
                btn.Text = "НЭВТРЭХ";
            }
        }
    }
}
