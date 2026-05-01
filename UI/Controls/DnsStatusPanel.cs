using System;
using System.Drawing;
using System.Windows.Forms;
using SshTunnelApp.Services;

namespace SshTunnelApp.UI.Controls
{
    public class DnsStatusPanel : Panel
    {
        private Button btnCheckDns;
        private Label lblDnsStatus;

        public DnsStatusPanel(PodkopDnsService dnsService, MainForm mainForm)
        {
            this.Size = new Size(500, 100);
            InitializeComponents(dnsService, mainForm);
        }

        private void InitializeComponents(PodkopDnsService dnsService, MainForm mainForm)
        {
            btnCheckDns = new Button
            {
                Text = "Проверить DNS",
                Location = new Point(5, 5),
                Size = new Size(120, 25),
                Enabled = false
            };
            btnCheckDns.Click += async (s, e) =>
            {
                btnCheckDns.Enabled = false;
                lblDnsStatus.Text = "Запрос...";
                try
                {
                    var dns = await dnsService.GetDnsStatusAsync();
                    if (dns != null)
                    {
                        lblDnsStatus.Text =
                            $"DNS: {dns.DnsStatusText}\n" +
                            $"Сервер: {dns.DnsServer} (тип: {dns.DnsType})\n" +
                            $"Bootstrap: {dns.BootstrapDnsServer} ({dns.BootstrapStatusText})\n" +
                            $"DHCP: {dns.DhcpConfigText} | {dns.DnsOnRouterText}";
                    }
                    else
                    {
                        lblDnsStatus.Text = "Не удалось получить состояние DNS";
                    }
                }
                catch (Exception ex)
                {
                    lblDnsStatus.Text = $"Ошибка: {ex.Message}";
                }
                finally
                {
                    btnCheckDns.Enabled = true;
                }
            };

            lblDnsStatus = new Label
            {
                Text = "DNS не проверен",
                Location = new Point(5, 35),
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Regular)
            };

            Controls.AddRange(new Control[] { btnCheckDns, lblDnsStatus });
        }

        public void SetButtonsEnabled(bool enabled)
        {
            btnCheckDns.Enabled = enabled;
        }
    }
}