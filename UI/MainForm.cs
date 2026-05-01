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

        public MainForm()
        {
            Text = "Podkop Manager";
            Size = new Size(640, 1150);   // Увеличена высота для прокси-панели
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            Ssh = new SshService();
            PodkopStatus = new PodkopStatusService(Ssh);
            PodkopTunnel = new PodkopTunnelService(Ssh);
            PodkopEditor = new PodkopTunnelEditorService(Ssh);
            RouterManager = new RouterManagerService();
            PodkopDns = new PodkopDnsService(Ssh);
            PodkopProxy = new PodkopProxyService(Ssh);

            routerSelector = new RouterSelectorPanel(RouterManager);
            routerSelector.RouterSelected += async (router) => await ConnectToRouter(router);

            controlPanel = new PodkopControlPanel(PodkopStatus, this);
            dnsPanel = new DnsStatusPanel(PodkopDns, this);

            var editPanel = new TunnelEditPanel(PodkopEditor, this);
            tunnelPanel = new TunnelManagementPanel(PodkopTunnel, editPanel, this);

            proxyPanel = new ProxyStatusPanel(PodkopProxy, this);

            // Вертикальное размещение
            routerSelector.Location = new Point(10, 10);
            controlPanel.Location = new Point(10, routerSelector.Bottom + 10);
            dnsPanel.Location = new Point(10, controlPanel.Bottom + 10);
            tunnelPanel.Location = new Point(10, dnsPanel.Bottom + 10);
            proxyPanel.Location = new Point(10, tunnelPanel.Bottom + 10);

            Controls.AddRange(new Control[] { routerSelector, controlPanel, dnsPanel, tunnelPanel, proxyPanel });
        }

        private async Task ConnectToRouter(RouterConnection router)
        {
            SetStatusText("Подключение...");
            if (Ssh.IsConnected)
                Ssh.Disconnect();

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