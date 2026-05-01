using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SshTunnelApp.UI.Controls;
using SshTunnelApp.Services;
using SshTunnelApp.Models;

namespace SshTunnelApp.UI
{
    public class MainForm : Form
    {
        public SshService Ssh { get; private set; }
        public PodkopStatusService PodkopStatus { get; private set; }
        public PodkopTunnelService PodkopTunnel { get; private set; }
        public PodkopTunnelEditorService PodkopEditor { get; private set; }
        public RouterManagerService RouterManager { get; private set; }
        public PodkopDnsService PodkopDns { get; private set; }
        public PodkopProxyService PodkopProxy { get; private set; }

        private RouterSelectorPanel routerSelector;
        private PodkopControlPanel controlPanel;
        private DnsStatusPanel dnsPanel;
        private TunnelManagementPanel tunnelPanel;
        private ProxyStatusPanel proxyPanel;

        private Label lblRouter, lblPodkopDns, lblTunnels, lblProxy;
        private TableLayoutPanel podkopDnsRow;

        public MainForm()
        {
            Text = "Podkop Manager";
            AutoScroll = true;
            MinimumSize = new Size(800, 600);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;

            // Сервисы
            Ssh = new SshService();
            PodkopStatus = new PodkopStatusService(Ssh);
            PodkopTunnel = new PodkopTunnelService(Ssh);
            PodkopEditor = new PodkopTunnelEditorService(Ssh);
            RouterManager = new RouterManagerService();
            PodkopDns = new PodkopDnsService(Ssh);
            PodkopProxy = new PodkopProxyService(Ssh);

            // Заголовки
            lblRouter = new Label { Text = "Роутер", Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };
            lblPodkopDns = new Label { Text = "Статус Podkop и DNS", Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };
            lblTunnels = new Label { Text = "Туннели", Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };
            lblProxy = new Label { Text = "Прокси (Outbounds)", Font = new Font("Arial", 10, FontStyle.Bold), AutoSize = true };

            // Панель выбора роутера
            routerSelector = new RouterSelectorPanel(RouterManager);
            routerSelector.RouterSelected += async (router) => await ConnectToRouter(router);

            // Панель статуса Podkop (с кнопками управления службой)
            var podkopManager = new PodkopServiceManager(Ssh);
            controlPanel = new PodkopControlPanel(PodkopStatus, podkopManager, this);

            // DNS‑панель
            dnsPanel = new DnsStatusPanel(PodkopDns, this);

            // Объединяем Podkop и DNS в одной строке
            podkopDnsRow = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            podkopDnsRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            podkopDnsRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            podkopDnsRow.Controls.Add(controlPanel, 0, 0);
            podkopDnsRow.Controls.Add(dnsPanel, 1, 0);

            // Туннели
            var editPanel = new TunnelEditPanel(PodkopEditor, this);
            tunnelPanel = new TunnelManagementPanel(PodkopTunnel, editPanel, this);
            tunnelPanel.SizeChanged += (s, e) => RepositionProxyPanel();

            // Прокси
            proxyPanel = new ProxyStatusPanel(PodkopProxy, this);

            // Начальное размещение
            int y = 10;
            lblRouter.Location = new Point(10, y); y += 20;
            routerSelector.Location = new Point(10, y); y += routerSelector.Height + 10;

            lblPodkopDns.Location = new Point(10, y); y += 20;
            podkopDnsRow.Location = new Point(10, y); y += podkopDnsRow.Height + 10;

            lblTunnels.Location = new Point(10, y); y += 20;
            tunnelPanel.Location = new Point(10, y); y += tunnelPanel.Height + 10;

            lblProxy.Location = new Point(10, y); y += 20;
            proxyPanel.Location = new Point(10, y);

            Controls.AddRange(new Control[] { lblRouter, routerSelector,
                                              lblPodkopDns, podkopDnsRow,
                                              lblTunnels, tunnelPanel,
                                              lblProxy, proxyPanel });
        }

        private void RepositionProxyPanel()
        {
            lblProxy.Location = new Point(10, tunnelPanel.Bottom + 10);
            proxyPanel.Location = new Point(10, lblProxy.Bottom + 10);
        }

        private async Task ConnectToRouter(RouterConnection router)
        {
            SetStatusText("Подключение...");
            if (Ssh.IsConnected) Ssh.Disconnect();

            try
            {
                await Ssh.ConnectAsync(router.Host, router.Port, router.Username, router.Password);
                SetStatusText("Подключено");
                controlPanel.SetButtonsEnabled(true);
                dnsPanel.SetButtonsEnabled(true);
                tunnelPanel.SetButtonsEnabled(true);
                proxyPanel.SetButtonsEnabled(true);
            }
            catch (Exception ex)
            {
                SetStatusText($"Ошибка подключения: {ex.Message}");
                MessageBox.Show($"Не удалось подключиться: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetStatusText(string text)
        {
            if (InvokeRequired)
                Invoke((MethodInvoker)(() => Text = $"Podkop Manager - {text}"));
            else
                Text = $"Podkop Manager - {text}";
        }
    }
}