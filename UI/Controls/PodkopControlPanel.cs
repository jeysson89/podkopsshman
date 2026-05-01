using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using SshTunnelApp.Services;

namespace SshTunnelApp.UI.Controls
{
    public class PodkopControlPanel : Panel
    {
        private Button btnGetStatus;
        private Panel statusIndicator;
        private Label lblIndicatorText;
        private PodkopStatusService statusService;
        private PodkopServiceManager serviceManager;   // новый сервис управления
        private MainForm mainForm;

        private Button btnStart, btnStop, btnReload, btnRestart, btnRestartDns;

        public PodkopControlPanel(PodkopStatusService status, PodkopServiceManager svcManager, MainForm form)
        {
            statusService = status;
            serviceManager = svcManager;
            mainForm = form;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.Padding = new Padding(5, 5, 5, 5);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var flow = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                WrapContents = false
            };

            // Кнопка проверки статуса
            btnGetStatus = new Button
            {
                Text = "Статус Podkop",
                Size = new Size(110, 25),
                Enabled = false
            };
            btnGetStatus.Click += BtnGetStatus_Click;

            // Индикатор статуса
            statusIndicator = new Panel
            {
                Size = new Size(100, 25),
                BorderStyle = BorderStyle.FixedSingle
            };
            lblIndicatorText = new Label
            {
                Text = "Не проверен",
                AutoSize = false,
                Size = new Size(96, 21),
                TextAlign = ContentAlignment.MiddleCenter
            };
            statusIndicator.Controls.Add(lblIndicatorText);
            SetIndicatorGray();

            // Кнопки управления службой
            btnStart = CreateServiceButton("Start", async () => await ExecuteAndRefresh(serviceManager.StartAsync));
            btnStop = CreateServiceButton("Stop", async () => await ExecuteAndRefresh(serviceManager.StopAsync));
            btnReload = CreateServiceButton("Reload", async () => await ExecuteAndRefresh(serviceManager.ReloadAsync));
            btnRestart = CreateServiceButton("Restart", async () => await ExecuteAndRefresh(serviceManager.RestartAsync));
            btnRestartDns = CreateServiceButton("Restart DNS", async () => await ExecuteAndRefresh(serviceManager.RestartDnsAsync));

            // Добавляем всё в flow layout
            flow.Controls.Add(btnGetStatus);
            flow.Controls.Add(statusIndicator);
            flow.Controls.Add(btnStart);
            flow.Controls.Add(btnStop);
            flow.Controls.Add(btnReload);
            flow.Controls.Add(btnRestart);
            flow.Controls.Add(btnRestartDns);

            this.Controls.Add(flow);
        }

        private Button CreateServiceButton(string text, Func<Task> action)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(75, 25),
                Enabled = false,
                Margin = new Padding(3, 0, 3, 0)
            };
            btn.Click += async (s, e) =>
            {
                btn.Enabled = false;
                try { await action(); } catch (Exception ex) { MessageBox.Show(ex.Message); }
                btn.Enabled = true;
            };
            return btn;
        }

        private async Task ExecuteAndRefresh(Func<Task> serviceAction)
        {
            mainForm.SetStatusText("Выполнение команды...");
            try
            {
                await serviceAction();
                // после команды обновляем статус
                await CheckStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                mainForm.SetStatusText("Готово");
            }
        }

        public void SetButtonsEnabled(bool enabled)
        {
            btnGetStatus.Enabled = enabled;
            bool svcEnabled = enabled;
            btnStart.Enabled = svcEnabled;
            btnStop.Enabled = svcEnabled;
            btnReload.Enabled = svcEnabled;
            btnRestart.Enabled = svcEnabled;
            btnRestartDns.Enabled = svcEnabled;
        }

        private async void BtnGetStatus_Click(object sender, EventArgs e)
        {
            await CheckStatus();
        }

        private async Task CheckStatus()
        {
            btnGetStatus.Enabled = false;
            SetIndicatorGray();
            mainForm.SetStatusText("Запрос статуса...");

            try
            {
                string status = await statusService.GetStatusAsync();
                if (status == "running")
                    SetIndicatorGreen("Запущен");
                else
                    SetIndicatorRed("Не запущен");
            }
            catch (Exception ex)
            {
                SetIndicatorRed($"Ошибка: {ex.Message}");
            }
            finally
            {
                btnGetStatus.Enabled = true;
                mainForm.SetStatusText("Статус получен");
            }
        }

        private void SetIndicatorGray()
        {
            statusIndicator.BackColor = Color.LightGray;
            lblIndicatorText.Text = "Проверка...";
            lblIndicatorText.ForeColor = Color.Black;
        }

        private void SetIndicatorGreen(string text)
        {
            statusIndicator.BackColor = Color.LightGreen;
            lblIndicatorText.Text = text;
            lblIndicatorText.ForeColor = Color.DarkGreen;
        }

        private void SetIndicatorRed(string text)
        {
            statusIndicator.BackColor = Color.LightCoral;
            lblIndicatorText.Text = text;
            lblIndicatorText.ForeColor = Color.DarkRed;
        }
    }
}