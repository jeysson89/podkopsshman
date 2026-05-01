using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SshTunnelApp.Helpers;
using SshTunnelApp.Services;

namespace SshTunnelApp.UI.Controls
{
    public class TunnelEditPanel : Panel
    {
        private TextBox txtTunnelDetails;
        private Button btnRefreshDetails, btnAddUrltestLink, btnDeleteUrltestLink, btnEditUrltestLink, btnSetProxyString;
        private Label lblType;
        private ListBox lstUrltestLinks;
        private string? currentTunnelName;
        private PodkopTunnelEditorService editorService;

        public TunnelEditPanel(PodkopTunnelEditorService editorSvc, MainForm form)
        {
            editorService = editorSvc;
            this.Size = new Size(580, 360);   // увеличена высота
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Тип туннеля
            lblType = new Label { Text = "Тип: не выбран", Location = new Point(10, 10), AutoSize = true };

            // Текстовое поле с деталями (увеличена высота до 120)
            txtTunnelDetails = new TextBox
            {
                Location = new Point(10, 35),
                Size = new Size(470, 120),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9)
            };

            // Кнопка "Обновить детали" – справа от текстового поля
            btnRefreshDetails = new Button
            {
                Text = "Обновить",
                Location = new Point(490, 35),
                Size = new Size(80, 25)
            };
            btnRefreshDetails.Click += async (s, e) => await LoadTunnelDetails(currentTunnelName);

            // Список ссылок urltest (увеличена высота до 120)
            lstUrltestLinks = new ListBox
            {
                Location = new Point(10, 165),
                Size = new Size(380, 120),
                Visible = false
            };

            // Кнопки для urltest
            btnAddUrltestLink = new Button
            {
                Text = "Добавить URL",
                Location = new Point(400, 165),
                Size = new Size(100, 25),
                Visible = false
            };
            btnAddUrltestLink.Click += BtnAddUrltestLink_Click;

            btnDeleteUrltestLink = new Button
            {
                Text = "Удалить",
                Location = new Point(400, 195),
                Size = new Size(100, 25),
                Visible = false,
                Enabled = false
            };
            btnDeleteUrltestLink.Click += BtnDeleteUrltestLink_Click;

            btnEditUrltestLink = new Button
            {
                Text = "Изменить",
                Location = new Point(400, 225),
                Size = new Size(100, 25),
                Visible = false,
                Enabled = false
            };
            btnEditUrltestLink.Click += BtnEditUrltestLink_Click;

            // Кнопка для proxy_string (внизу)
            btnSetProxyString = new Button
            {
                Text = "Изменить proxy_string",
                Location = new Point(10, 320),
                Size = new Size(170, 25),
                Visible = false
            };
            btnSetProxyString.Click += BtnSetProxyString_Click;

            // Логика выбора ссылки
            lstUrltestLinks.SelectedIndexChanged += (s, e) =>
            {
                bool hasSel = lstUrltestLinks.SelectedItem != null;
                btnDeleteUrltestLink.Enabled = hasSel;
                btnEditUrltestLink.Enabled = hasSel;
            };

            Controls.AddRange(new Control[] { lblType, txtTunnelDetails, btnRefreshDetails,
                                              lstUrltestLinks, btnAddUrltestLink, btnDeleteUrltestLink, btnEditUrltestLink,
                                              btnSetProxyString });
        }

        public async Task LoadTunnelDetails(string? tunnelName)
        {
            currentTunnelName = tunnelName;
            if (string.IsNullOrEmpty(tunnelName))
            {
                txtTunnelDetails.Text = "Нет выбранного туннеля.";
                lblType.Text = "Тип: не выбран";
                HideAllControls();
                return;
            }

            try
            {
                string output = await editorService.RunCommandAsync($"uci show podkop.{tunnelName}");
                txtTunnelDetails.Text = output.Replace("\n", Environment.NewLine);

                bool isUrltest = output.Contains("proxy_config_type") &&
                                 (output.Contains("'urltest'") || output.Contains("\"urltest\""));
                bool isProxy = (output.Contains("connection_type") &&
                                (output.Contains("'proxy'") || output.Contains("\"proxy\"")))
                               || output.Contains("proxy_string=");

                lblType.Text = isUrltest ? "Тип: urltest (url)" :
                               isProxy ? "Тип: proxy" : "Тип: обычный";

                btnSetProxyString.Visible = isProxy;
                lstUrltestLinks.Visible = isUrltest;
                btnAddUrltestLink.Visible = isUrltest;
                btnDeleteUrltestLink.Visible = isUrltest;
                btnEditUrltestLink.Visible = isUrltest;

                if (isUrltest)
                {
                    var links = await editorService.GetUrltestLinksAsync(tunnelName);
                    lstUrltestLinks.Items.Clear();
                    lstUrltestLinks.Items.AddRange(links.ToArray());
                }
            }
            catch (Exception ex)
            {
                txtTunnelDetails.Text = $"Ошибка загрузки: {ex.Message}";
                lblType.Text = "Ошибка";
                HideAllControls();
            }
        }

        private void HideAllControls()
        {
            btnSetProxyString.Visible = false;
            lstUrltestLinks.Visible = false;
            btnAddUrltestLink.Visible = false;
            btnDeleteUrltestLink.Visible = false;
            btnEditUrltestLink.Visible = false;
        }

        private async void BtnAddUrltestLink_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentTunnelName)) return;
            string newLink = Prompt.ShowDialog("Введите новый URL (ss://...):", "Добавить ссылку");
            if (string.IsNullOrWhiteSpace(newLink)) return;
            try
            {
                await editorService.AddUrltestLinkAsync(currentTunnelName, newLink);
                await LoadTunnelDetails(currentTunnelName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnDeleteUrltestLink_Click(object? sender, EventArgs e)
        {
            if (lstUrltestLinks.SelectedItem == null || string.IsNullOrEmpty(currentTunnelName)) return;
            string link = lstUrltestLinks.SelectedItem.ToString()!;
            if (MessageBox.Show($"Удалить ссылку?\n{link}", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                await editorService.DeleteUrltestLinkAsync(currentTunnelName, link);
                await LoadTunnelDetails(currentTunnelName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnEditUrltestLink_Click(object? sender, EventArgs e)
        {
            if (lstUrltestLinks.SelectedItem == null || string.IsNullOrEmpty(currentTunnelName)) return;
            string oldLink = lstUrltestLinks.SelectedItem.ToString()!;
            string newLink = Prompt.ShowDialog($"Текущая ссылка:\n{oldLink}\n\nВведите новое значение:", "Изменить ссылку");
            if (string.IsNullOrWhiteSpace(newLink) || newLink == oldLink) return;
            try
            {
                await editorService.DeleteUrltestLinkAsync(currentTunnelName, oldLink);
                await editorService.AddUrltestLinkAsync(currentTunnelName, newLink);
                await LoadTunnelDetails(currentTunnelName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSetProxyString_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentTunnelName)) return;
            string current = "";
            if (txtTunnelDetails.Text.Contains("proxy_string="))
            {
                int start = txtTunnelDetails.Text.IndexOf("proxy_string=") + "proxy_string=".Length;
                int end = txtTunnelDetails.Text.IndexOf('\n', start);
                if (end == -1) end = txtTunnelDetails.Text.Length;
                current = txtTunnelDetails.Text[start..end].Trim();
            }
            string newProxy = Prompt.ShowDialog($"Текущий proxy_string: {current}\nВведите новое значение:", "Изменить proxy_string");
            if (string.IsNullOrWhiteSpace(newProxy)) return;
            try
            {
                await editorService.SetProxyStringAsync(currentTunnelName, newProxy);
                await LoadTunnelDetails(currentTunnelName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}