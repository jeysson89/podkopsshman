using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SshTunnelApp.Models;
using SshTunnelApp.Services;

namespace SshTunnelApp.UI.Controls
{
    public class RouterSelectorPanel : Panel
    {
        private ComboBox cmbRouters;
        private Button btnAdd, btnDelete;
        private RouterManagerService routerManager;
        private bool ignoreSelectionChange = false;

        public event Action<RouterConnection>? RouterSelected;

        public RouterSelectorPanel(RouterManagerService manager)
        {
            routerManager = manager;
            this.Size = new Size(600, 35);
            InitializeComponents();
            RefreshList();
        }

        private void InitializeComponents()
        {
            var lbl = new Label { Text = "Роутер:", Location = new Point(5, 7), AutoSize = true };
            cmbRouters = new ComboBox
            {
                Location = new Point(60, 5),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbRouters.SelectedIndexChanged += CmbRouters_SelectedIndexChanged;

            btnAdd = new Button { Text = "Добавить", Location = new Point(320, 3), Size = new Size(80, 25) };
            btnAdd.Click += BtnAdd_Click;

            btnDelete = new Button { Text = "Удалить", Location = new Point(410, 3), Size = new Size(80, 25) };
            btnDelete.Click += BtnDelete_Click;

            Controls.AddRange(new Control[] { lbl, cmbRouters, btnAdd, btnDelete });
        }

        private void RefreshList()
        {
            ignoreSelectionChange = true;
            cmbRouters.Items.Clear();
            var routers = routerManager.GetRouters();
            cmbRouters.Items.AddRange(routers.Select(r => r.Name).ToArray());
            if (cmbRouters.Items.Count > 0)
                cmbRouters.SelectedIndex = -1; // Ничего не выбрано
            ignoreSelectionChange = false;
        }

        private void CmbRouters_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (ignoreSelectionChange) return;

            if (cmbRouters.SelectedItem != null)
            {
                string name = cmbRouters.SelectedItem.ToString()!;
                RouterConnection? router = routerManager.GetRouter(name);
                if (router != null)
                    RouterSelected?.Invoke(router);
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            using (var form = new RouterEditForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    routerManager.AddRouter(form.Router);
                    RefreshList();
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (cmbRouters.SelectedItem == null) return;
            string name = cmbRouters.SelectedItem.ToString()!;
            if (MessageBox.Show($"Удалить роутер '{name}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                routerManager.DeleteRouter(name);
                RefreshList();
            }
        }
    }

    // Форма добавления роутера остаётся без изменений
    internal class RouterEditForm : Form
    {
        private TextBox txtName, txtHost, txtPort, txtUser, txtPass;
        public RouterConnection Router { get; private set; } = new();

        public RouterEditForm()
        {
            Text = "Новый роутер";
            Size = new Size(300, 230);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;

            var lblName = new Label { Text = "Имя:", Location = new Point(10, 10), AutoSize = true };
            txtName = new TextBox { Location = new Point(100, 7), Width = 150 };
            var lblHost = new Label { Text = "Хост:", Location = new Point(10, 40), AutoSize = true };
            txtHost = new TextBox { Location = new Point(100, 37), Width = 150 };
            var lblPort = new Label { Text = "Порт:", Location = new Point(10, 70), AutoSize = true };
            txtPort = new TextBox { Location = new Point(100, 67), Width = 50, Text = "22" };
            var lblUser = new Label { Text = "Пользователь:", Location = new Point(10, 100), AutoSize = true };
            txtUser = new TextBox { Location = new Point(100, 97), Width = 150, Text = "root" };
            var lblPass = new Label { Text = "Пароль:", Location = new Point(10, 130), AutoSize = true };
            txtPass = new TextBox { Location = new Point(100, 127), Width = 150, PasswordChar = '*' };

            var btnOk = new Button { Text = "OK", Location = new Point(100, 160), Size = new Size(80, 25) };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtHost.Text))
                {
                    MessageBox.Show("Имя и хост обязательны.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                Router = new RouterConnection
                {
                    Name = txtName.Text.Trim(),
                    Host = txtHost.Text.Trim(),
                    Port = int.TryParse(txtPort.Text, out int p) ? p : 22,
                    Username = txtUser.Text.Trim(),
                    Password = txtPass.Text
                };
                DialogResult = DialogResult.OK;
                Close();
            };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(190, 160), Size = new Size(80, 25) };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.AddRange(new Control[] { lblName, txtName, lblHost, txtHost, lblPort, txtPort,
                                              lblUser, txtUser, lblPass, txtPass, btnOk, btnCancel });
        }
    }
}