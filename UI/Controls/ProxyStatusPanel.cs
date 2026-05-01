using System;
using System.Drawing;
using System.Windows.Forms;
using SshTunnelApp.Services;

namespace SshTunnelApp.UI.Controls
{
    public class ProxyStatusPanel : Panel
    {
        private Button btnRefresh;
        private DataGridView dgvProxies;
        private PodkopProxyService proxyService;
        private MainForm mainForm;

        public ProxyStatusPanel(PodkopProxyService proxySvc, MainForm form)
        {
            proxyService = proxySvc;
            mainForm = form;
            this.Size = new Size(580, 280);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            btnRefresh = new Button
            {
                Text = "Обновить Outbounds",
                Location = new Point(5, 5),
                Size = new Size(150, 25),
                Enabled = false
            };
            btnRefresh.Click += async (s, e) => await RefreshProxies();

            dgvProxies = new DataGridView
            {
                Location = new Point(5, 35),
                Size = new Size(570, 240),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Настраиваем колонки
            dgvProxies.Columns.Add("Name", "Имя");
            dgvProxies.Columns.Add("Type", "Тип");
            dgvProxies.Columns.Add("Delay", "Задержка");
            dgvProxies.Columns.Add("State", "Состояние");

            // Обработчик для раскрашивания строк
            dgvProxies.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < dgvProxies.Rows.Count)
                {
                    var row = dgvProxies.Rows[e.RowIndex];
                    var proxy = row.Tag as Models.ProxyInfo;
                    if (proxy != null)
                    {
                        Color backColor = Color.LightGray; // нет данных
                        string stateText = "Нет данных";

                        if (proxy.HasData)
                        {
                            int delay = proxy.LastDelay.Value;
                            if (delay < 500)
                            {
                                backColor = Color.LightGreen;
                                stateText = "Доступен";
                            }
                            else if (delay < 1000)
                            {
                                backColor = Color.LightYellow;
                                stateText = "Замедлен";
                            }
                            else
                            {
                                backColor = Color.LightCoral;
                                stateText = "Большая задержка";
                            }
                        }

                        row.DefaultCellStyle.BackColor = backColor;
                        row.Cells["State"].Value = stateText;
                    }
                }
            };

            Controls.AddRange(new Control[] { btnRefresh, dgvProxies });
        }

        private async System.Threading.Tasks.Task RefreshProxies()
        {
            btnRefresh.Enabled = false;
            mainForm.SetStatusText("Запрос состояния прокси...");
            dgvProxies.Rows.Clear();

            try
            {
                var proxies = await proxyService.GetProxiesAsync();
                foreach (var p in proxies)
                {
                    int rowIndex = dgvProxies.Rows.Add(p.Name, p.Type, p.DelayDisplay, "");
                    dgvProxies.Rows[rowIndex].Tag = p; // сохраняем объект для форматирования
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения состояния прокси: {ex.Message}", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefresh.Enabled = true;
                mainForm.SetStatusText("Статус Outbounds обновлён");
            }
        }

        public void SetButtonsEnabled(bool enabled)
        {
            btnRefresh.Enabled = enabled;
            if (!enabled) dgvProxies.Rows.Clear();
        }
    }
}