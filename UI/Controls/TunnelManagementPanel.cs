using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SshTunnelApp.Helpers;
using SshTunnelApp.Services;

namespace SshTunnelApp.UI.Controls
{
    public class TunnelManagementPanel : Panel
    {
        private ListBox listTunnels;
        private Button btnDetails, btnAdd, btnDelete;
        private PodkopTunnelService tunnelService;
        private TunnelEditPanel editPanel;
        private MainForm mainForm;

        public TunnelManagementPanel(PodkopTunnelService tunnelSvc, TunnelEditPanel tunnelEditPanel, MainForm form)
        {
            tunnelService = tunnelSvc;
            editPanel = tunnelEditPanel;
            mainForm = form;
            this.Size = new Size(600, 580);   // увеличена высота до 580
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            listTunnels = new ListBox
            {
                Location = new Point(5, 5),
                Size = new Size(200, 150),
                IntegralHeight = false
            };
            listTunnels.SelectedIndexChanged += ListTunnels_SelectedIndexChanged;

            btnDetails = new Button { Text = "Подробнее", Location = new Point(5, 160), Size = new Size(90, 25), Enabled = false };
            btnDetails.Click += BtnDetails_Click;

            btnAdd = new Button { Text = "Добавить", Location = new Point(100, 160), Size = new Size(90, 25) };
            btnAdd.Click += BtnAdd_Click;

            btnDelete = new Button { Text = "Удалить", Location = new Point(195, 160), Size = new Size(90, 25), Enabled = false };
            btnDelete.Click += BtnDelete_Click;

            // Панель редактирования размещаем с достаточным отступом
            editPanel.Location = new Point(5, 195);

            Controls.AddRange(new Control[] { listTunnels, btnDetails, btnAdd, btnDelete, editPanel });
        }

        private void ListTunnels_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool hasSelection = listTunnels.SelectedItem != null;
            btnDetails.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;

            if (hasSelection)
                _ = editPanel.LoadTunnelDetails(listTunnels.SelectedItem.ToString()!);
            else
                _ = editPanel.LoadTunnelDetails(null);
        }

        private async void BtnDetails_Click(object? sender, EventArgs e)
        {
            if (listTunnels.SelectedItem == null) return;
            await editPanel.LoadTunnelDetails(listTunnels.SelectedItem.ToString()!);
        }

        private async void BtnAdd_Click(object? sender, EventArgs e)
        {
            string tunnelName = Prompt.ShowDialog("Имя нового туннеля:", "Добавление");
            if (string.IsNullOrWhiteSpace(tunnelName)) return;
            string remoteHost = Prompt.ShowDialog("Удалённый хост (если нужно):", "Добавление");
            string remotePort = Prompt.ShowDialog("Удалённый порт:", "Добавление");
            string localPort = Prompt.ShowDialog("Локальный порт:", "Добавление");

            try
            {
                await tunnelService.AddTunnelAsync(tunnelName, remoteHost, remotePort, localPort);
                await RefreshTunnelList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (listTunnels.SelectedItem == null) return;
            string tunnelName = listTunnels.SelectedItem.ToString()!;
            if (MessageBox.Show($"Удалить туннель '{tunnelName}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                await tunnelService.DeleteTunnelAsync(tunnelName);
                await RefreshTunnelList();
                await editPanel.LoadTunnelDetails(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task RefreshTunnelList()
        {
            var tunnels = await tunnelService.GetTunnelNamesAsync();
            listTunnels.Items.Clear();
            listTunnels.Items.AddRange(tunnels.ToArray());
        }

        public void SetButtonsEnabled(bool enabled)
        {
            if (enabled)
                _ = RefreshTunnelList();
            else
                listTunnels.Items.Clear();
        }
    }
}