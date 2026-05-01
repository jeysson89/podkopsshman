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
        private MainForm mainForm;

        public PodkopControlPanel(PodkopStatusService status, MainForm form)
        {
            statusService = status;
            mainForm = form;
            this.Size = new Size(600, 50);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            btnGetStatus = new Button
            {
                Text = "Статус Podkop",
                Location = new Point(5, 5),
                Size = new Size(120, 25),
                Enabled = false
            };
            btnGetStatus.Click += BtnGetStatus_Click;

            // Индикатор статуса (цветной прямоугольник)
            statusIndicator = new Panel
            {
                Location = new Point(135, 5),
                Size = new Size(150, 25),
                BorderStyle = BorderStyle.FixedSingle
            };
            lblIndicatorText = new Label
            {
                Text = "Не проверен",
                Location = new Point(2, 2),
                AutoSize = false,
                Size = new Size(146, 21),
                TextAlign = ContentAlignment.MiddleCenter
            };
            statusIndicator.Controls.Add(lblIndicatorText);
            SetIndicatorGray();

            Controls.AddRange(new Control[] { btnGetStatus, statusIndicator });
        }

        public void SetButtonsEnabled(bool enabled)
        {
            btnGetStatus.Enabled = enabled;
        }

        private async void BtnGetStatus_Click(object sender, EventArgs e)
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