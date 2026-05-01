using System;
using System.Drawing;
using System.Windows.Forms;

namespace SshTunnelApp.UI.Controls
{
    public class ConnectionPanel : Panel
    {
        private TextBox txtHost, txtPort, txtUsername, txtPassword;
        private Button btnConnect;
        private Label lblStatus;
        private SshService sshService;

        public event Action? Connected;

        public ConnectionPanel(SshService ssh)
        {
            sshService = ssh;
            this.Size = new Size(460, 100);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Метки и поля
            var lblHost = new Label { Text = "SSH Host:", Location = new Point(5, 5), AutoSize = true };
            txtHost = new TextBox { Location = new Point(80, 2), Width = 120, Text = "192.168.41.1" };

            var lblPort = new Label { Text = "Port:", Location = new Point(210, 5), AutoSize = true };
            txtPort = new TextBox { Location = new Point(240, 2), Width = 50, Text = "22" };

            var lblUser = new Label { Text = "Username:", Location = new Point(5, 35), AutoSize = true };
            txtUsername = new TextBox { Location = new Point(80, 32), Width = 120, Text = "root" };

            var lblPass = new Label { Text = "Password:", Location = new Point(210, 35), AutoSize = true };
            txtPassword = new TextBox { Location = new Point(270, 32), Width = 110, PasswordChar = '*' };

            btnConnect = new Button { Text = "Подключиться", Location = new Point(5, 65), Size = new Size(110, 25) };
            btnConnect.Click += BtnConnect_Click;

            lblStatus = new Label { Text = "Не подключено", Location = new Point(120, 68), AutoSize = true, ForeColor = Color.Red };

            Controls.AddRange(new Control[] { lblHost, txtHost, lblPort, txtPort,
                                              lblUser, txtUsername, lblPass, txtPassword,
                                              btnConnect, lblStatus });
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            btnConnect.Enabled = false;
            lblStatus.Text = "Подключение...";
            lblStatus.ForeColor = Color.Blue;

            try
            {
                await sshService.ConnectAsync(
                    txtHost.Text.Trim(),
                    int.TryParse(txtPort.Text, out int port) ? port : 22,
                    txtUsername.Text.Trim(),
                    txtPassword.Text
                );

                lblStatus.Text = "Подключено";
                lblStatus.ForeColor = Color.Green;

                Connected?.Invoke();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Ошибка: {ex.Message}";
                lblStatus.ForeColor = Color.Red;
                btnConnect.Enabled = true;
            }
        }

        // Заполняет поля данными (используется при выборе роутера из списка)
        public void SetConnectionInfo(string host, int port, string username, string password)
        {
            txtHost.Text = host;
            txtPort.Text = port.ToString();
            txtUsername.Text = username;
            txtPassword.Text = password;
        }

        public bool IsConnected => sshService.IsConnected;
    }
}