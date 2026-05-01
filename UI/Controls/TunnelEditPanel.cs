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

        private Label lblDomains;
        private TextBox txtDomains;
        private Button btnSaveDomains, btnMassAddDomains, btnClearDomains;

        public TunnelEditPanel(PodkopTunnelEditorService editorSvc, MainForm form)
        {
            editorService = editorSvc;
            this.Size = new Size(580, 500);
            this.Visible = false;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            lblType = new Label { Text = "Тип: не выбран", Location = new Point(10, 10), AutoSize = true };

            txtTunnelDetails = new TextBox
            {
                Location = new Point(10, 35),
                Size = new Size(470, 120),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9)
            };

            btnRefreshDetails = new Button
            {
                Text = "Обновить",
                Location = new Point(490, 35),
                Size = new Size(80, 25)
            };
            btnRefreshDetails.Click += async (s, e) => await LoadTunnelDetails(currentTunnelName);

            lstUrltestLinks = new ListBox
            {
                Location = new Point(10, 165),
                Size = new Size(380, 120),
                Visible = false
            };

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

            btnSetProxyString = new Button
            {
                Text = "Изменить proxy_string",
                Location = new Point(10, 320),
                Size = new Size(170, 25),
                Visible = false
            };
            btnSetProxyString.Click += BtnSetProxyString_Click;

            lstUrltestLinks.SelectedIndexChanged += (s, e) =>
            {
                bool hasSel = lstUrltestLinks.SelectedItem != null;
                btnDeleteUrltestLink.Enabled = hasSel;
                btnEditUrltestLink.Enabled = hasSel;
            };

            lblDomains = new Label
            {
                Text = "Домены (user_domains_text)",
                Location = new Point(10, 355),
                AutoSize = true,
                Visible = false
            };

            txtDomains = new TextBox
            {
                Location = new Point(10, 375),
                Size = new Size(470, 70),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Visible = false,
                Font = new Font("Consolas", 9)
            };

            btnSaveDomains = new Button
            {
                Text = "Сохранить домены",
                Location = new Point(490, 375),
                Size = new Size(80, 25),
                Visible = false
            };
            btnSaveDomains.Click += async (s, e) => await SaveDomains();

            btnMassAddDomains = new Button
            {
                Text = "Массово добавить",
                Location = new Point(490, 405),
                Size = new Size(80, 25),
                Visible = false
            };
            btnMassAddDomains.Click += BtnMassAddDomains_Click;

            btnClearDomains = new Button
            {
                Text = "Очистить",
                Location = new Point(490, 435),
                Size = new Size(80, 25),
                Visible = false
            };
            btnClearDomains.Click += async (s, e) =>
            {
                if (MessageBox.Show("Очистить все домены?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    txtDomains.Text = "";
                    await SaveDomains();
                }
            };

            Controls.AddRange(new Control[] {
                lblType, txtTunnelDetails, btnRefreshDetails,
                lstUrltestLinks, btnAddUrltestLink, btnDeleteUrltestLink, btnEditUrltestLink,
                btnSetProxyString,
                lblDomains, txtDomains, btnSaveDomains, btnMassAddDomains, btnClearDomains
            });
        }

        public async Task LoadTunnelDetails(string? tunnelName)
        {
            currentTunnelName = tunnelName;
            if (string.IsNullOrEmpty(tunnelName))
            {
                this.Visible = false;
                txtTunnelDetails.Text = "";
                lblType.Text = "Тип: не выбран";
                lstUrltestLinks.Items.Clear();
                HideAllControls();
                return;
            }

            this.Visible = true;
            this.Size = new Size(580, 500);
            try
            {
                string output = await editorService.RunCommandAsync($"uci show podkop.{tunnelName}");
                txtTunnelDetails.Text = output.Replace("\n", Environment.NewLine);

                bool isUrltest = output.Contains("proxy_config_type") &&
                                 (output.Contains("'urltest'") || output.Contains("\"urltest\""));
                bool isProxy = (output.Contains("connection_type") &&
                                (output.Contains("'proxy'") || output.Contains("\"proxy\"")))
                               || output.Contains("proxy_string=");

                bool hasDomains = output.Contains("user_domains_text=");

                lblType.Text = isUrltest ? "Тип: urltest (url)" :
                               isProxy ? "Тип: proxy" : "Тип: обычный";

                btnSetProxyString.Visible = isProxy;
                lstUrltestLinks.Visible = isUrltest;
                btnAddUrltestLink.Visible = isUrltest;
                btnDeleteUrltestLink.Visible = isUrltest;
                btnEditUrltestLink.Visible = isUrltest;
                btnRefreshDetails.Visible = true;

                bool showDomains = hasDomains;
                lblDomains.Visible = showDomains;
                txtDomains.Visible = showDomains;
                btnSaveDomains.Visible = showDomains;
                btnMassAddDomains.Visible = showDomains;
                btnClearDomains.Visible = showDomains;

                if (isUrltest)
                {
                    var links = await editorService.GetUrltestLinksAsync(tunnelName);
                    lstUrltestLinks.Items.Clear();
                    lstUrltestLinks.Items.AddRange(links.ToArray());
                }

                if (hasDomains)
                {
                    string domains = await editorService.GetUserDomainsTextAsync(tunnelName);
                    txtDomains.Text = domains.Replace("\n", Environment.NewLine);
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
            btnRefreshDetails.Visible = false;
            lblDomains.Visible = false;
            txtDomains.Visible = false;
            btnSaveDomains.Visible = false;
            btnMassAddDomains.Visible = false;
            btnClearDomains.Visible = false;
        }

        private async Task SaveDomains()
        {
            if (string.IsNullOrEmpty(currentTunnelName)) return;
            try
            {
                string normalized = editorService.NormalizeDomainsText(txtDomains.Text);
                await editorService.SetUserDomainsTextAsync(currentTunnelName, normalized);
                await LoadTunnelDetails(currentTunnelName);
                MessageBox.Show("Домены сохранены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения доменов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnMassAddDomains_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentTunnelName)) return;

            string input = Prompt.ShowDialog("Введите домены (через пробел, запятую или с новой строки):", "Массовое добавление");
            if (string.IsNullOrWhiteSpace(input)) return;

            string newNormalized = editorService.NormalizeDomainsText(input);
            if (string.IsNullOrEmpty(newNormalized)) return;

            string currentNormalized = editorService.NormalizeDomainsText(txtDomains.Text);
            var currentList = currentNormalized.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            var newList = newNormalized.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            bool added = false;
            foreach (var domain in newList)
            {
                if (!currentList.Contains(domain))
                {
                    currentList.Add(domain);
                    added = true;
                }
            }

            if (added)
            {
                string merged = string.Join("\n", currentList);
                await editorService.SetUserDomainsTextAsync(currentTunnelName, merged);
                await LoadTunnelDetails(currentTunnelName);
                MessageBox.Show("Новые домены добавлены.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Новых доменов не найдено.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ... остальные методы (AddUrltestLink, DeleteUrltestLink, EditUrltestLink, SetProxyString) без изменений ...
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