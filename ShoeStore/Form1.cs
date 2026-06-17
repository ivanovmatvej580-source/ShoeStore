using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ShoeStore
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        private int currentCustomerId = -1;
        private string currentCustomerLogin = "";
        private string currentUserRole = "guest";
        private bool isAdmin = false;

        private readonly Color DarkBg = Color.FromArgb(18, 18, 18);
        private readonly Color CardBg = Color.FromArgb(28, 28, 28);
        private readonly Color InputBg = Color.FromArgb(42, 42, 42);
        private readonly Color Accent = Color.FromArgb(59, 130, 246);
        private readonly Color Success = Color.FromArgb(34, 197, 94);
        private readonly Color Danger = Color.FromArgb(239, 68, 68);
        private readonly Color TextPrimary = Color.FromArgb(229, 229, 229);
        private readonly Color TextSecondary = Color.FromArgb(140, 140, 140);

        private Panel loginPanel, mainPanel;
        private TextBox txtLogin, txtPassword, txtSearch;
        private Button btnLogin, btnRegister, btnGuest;
        private Label lblStatus, lblUser, lblTotal, lblProductStatus, cartCountLabel;
        private TabControl tabControl;
        private DataGridView gridProducts, gridCart, gridUsers;
        private DataTable cartTable;

        private TextBox txtProdName, txtProdBrand, txtProdCategory, txtProdColor;
        private NumericUpDown numProdPrice, numProdStock;

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(int l, int t, int r, int b, int w, int h);

        public Form1()
        {
            InitializeComponent();
            this.Hide();
            ShowLogin();
        }

        private void InitializeComponent()
        {
            this.Text = "StyleStep";
            this.Size = new Size(1050, 620);
            this.MinimumSize = new Size(750, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DarkBg;
            this.Font = new Font("Segoe UI", 10);
            CreateLoginPanel();
            CreateMainPanel();
            this.Controls.Add(mainPanel);
            this.Controls.Add(loginPanel);
        }

        private void CreateLoginPanel()
        {
            loginPanel = new Panel { Dock = DockStyle.Fill, BackColor = DarkBg };
            Panel card = new Panel { Size = new Size(340, 320), BackColor = CardBg };
            card.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 340, 320, 14, 14));

            Label title = new Label { Text = "StyleStep", Font = new Font("Segoe UI", 26, FontStyle.Bold), ForeColor = Accent, Size = new Size(280, 40), Location = new Point(30, 25) };
            Label sub = new Label { Text = "Войдите в аккаунт", Font = new Font("Segoe UI", 10), ForeColor = TextSecondary, Location = new Point(30, 65), AutoSize = true };

            txtLogin = new TextBox { Location = new Point(30, 100), Size = new Size(280, 34), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Text = "admin" };
            txtLogin.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 280, 34, 7, 7));

            txtPassword = new TextBox { Location = new Point(30, 155), Size = new Size(280, 34), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 11), Text = "admin123", PasswordChar = '●' };
            txtPassword.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 280, 34, 7, 7));

            btnLogin = CreateBtn("Войти", Accent, 215, 300, 40);
            btnLogin.Click += BtnLogin_Click;

            btnRegister = CreateBtn("Регистрация", Success, 263, 145, 32);
            btnRegister.Click += BtnRegister_Click;

            btnGuest = CreateBtn("Гость", InputBg, 263, 185, 32);
            btnGuest.Click += BtnGuest_Click;

            lblStatus = new Label { Text = "", Font = new Font("Segoe UI", 8), ForeColor = TextSecondary, Location = new Point(30, 298), Size = new Size(280, 15), TextAlign = ContentAlignment.MiddleCenter };

            card.Controls.AddRange(new Control[] { title, sub, txtLogin, txtPassword, btnLogin, btnRegister, btnGuest, lblStatus });
            loginPanel.Controls.Add(card);
            this.Resize += (s, e) => card.Location = new Point((this.Width - 340) / 2, (this.Height - 320) / 2);
            card.Location = new Point((this.Width - 340) / 2, (this.Height - 320) / 2);
        }

        private Button CreateBtn(string text, Color color, int y, int w, int h)
        {
            Button btn = new Button { Text = text, Font = new Font("Segoe UI", text == "Войти" ? 11 : 9, text == "Войти" ? FontStyle.Bold : FontStyle.Regular), Location = new Point(text == "Войти" ? 30 : (text == "Регистрация" ? 30 : 175), y), Size = new Size(w, h), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btn.FlatAppearance.BorderSize = 0;
            btn.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, w, h, 7, 7));
            return btn;
        }

        private void CreateMainPanel()
        {
            mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = DarkBg, Visible = false };

            Panel topBar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(22, 22, 22), Padding = new Padding(12, 0, 12, 0) };
            Label logo = new Label { Text = "StyleStep", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Accent, Location = new Point(12, 10), AutoSize = true };
            lblUser = new Label { Text = "Гость", Font = new Font("Segoe UI", 8), ForeColor = TextSecondary, Location = new Point(120, 13), AutoSize = true };
            cartCountLabel = new Label { Text = "Корзина (0)", Font = new Font("Segoe UI", 8), ForeColor = TextSecondary, Location = new Point(this.Width - 170, 13), AutoSize = true };

            Button btnLogout = new Button { Text = "Выход", Location = new Point(this.Width - 70, 8), Size = new Size(55, 26), BackColor = Color.FromArgb(50, 50, 50), ForeColor = TextPrimary, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 7), Cursor = Cursors.Hand };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 55, 26, 5, 5));
            btnLogout.Click += (s, e) => Application.Restart();
            topBar.Controls.AddRange(new Control[] { logo, lblUser, cartCountLabel, btnLogout });

            tabControl = new TabControl { Dock = DockStyle.Fill, BackColor = DarkBg };
            mainPanel.Controls.Add(tabControl);
            mainPanel.Controls.Add(topBar);

            this.Resize += (s, e) => { if (mainPanel.Visible) { btnLogout.Location = new Point(this.Width - 70, 8); cartCountLabel.Location = new Point(this.Width - 170, 13); } };
        }

        private DataGridView CreateGrid()
        {
            var g = new DataGridView { BackgroundColor = CardBg, ForeColor = TextPrimary, GridColor = Color.FromArgb(45, 45, 45), BorderStyle = BorderStyle.None, AllowUserToAddRows = false, ReadOnly = true, RowHeadersVisible = false, EnableHeadersVisualStyles = false, ColumnHeadersHeight = 34, RowTemplate = { Height = 32 } };
            g.DefaultCellStyle.BackColor = CardBg;
            g.DefaultCellStyle.ForeColor = TextPrimary;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(59, 130, 246, 90);
            g.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            g.DefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 20, 20);
            g.ColumnHeadersDefaultCellStyle.ForeColor = TextSecondary;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            return g;
        }

        private TabPage CreateCatalogTab()
        {
            TabPage page = new TabPage("Каталог") { BackColor = DarkBg };
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = DarkBg };

            Panel header = new Panel { Dock = DockStyle.Top, Height = 55, BackColor = DarkBg };
            Label title = new Label { Text = "Каталог обуви", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = TextPrimary, Location = new Point(0, 2), AutoSize = true };

            txtSearch = new TextBox { Location = new Point(0, 28), Size = new Size(240, 26), BackColor = InputBg, ForeColor = TextSecondary, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9), Text = "Поиск..." };
            txtSearch.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 240, 26, 6, 6));
            txtSearch.Enter += (s, e) => { if (txtSearch.Text == "Поиск...") { txtSearch.Text = ""; txtSearch.ForeColor = TextPrimary; } };
            txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Поиск..."; txtSearch.ForeColor = TextSecondary; } };

            lblProductStatus = new Label { Text = "", Font = new Font("Segoe UI", 8), ForeColor = TextSecondary, Location = new Point(250, 33), AutoSize = true };
            header.Controls.AddRange(new Control[] { title, txtSearch, lblProductStatus });

            gridProducts = CreateGrid();
            gridProducts.Dock = DockStyle.Fill;
            gridProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProducts.MultiSelect = false;

            panel.Controls.Add(gridProducts);
            panel.Controls.Add(header);
            page.Controls.Add(panel);
            return page;
        }

        private TabPage CreateCartTab()
        {
            TabPage page = new TabPage("Корзина") { BackColor = DarkBg };
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = DarkBg };

            Label title = new Label { Text = "Корзина", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = TextPrimary, Location = new Point(0, 5), AutoSize = true };

            gridCart = CreateGrid();
            gridCart.Location = new Point(0, 45);
            gridCart.Size = new Size(panel.Width - 2, panel.Height - 120);
            gridCart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            DataGridViewButtonColumn btnR = new DataGridViewButtonColumn { Name = "Remove", HeaderText = "", Text = "✕", UseColumnTextForButtonValue = true, Width = 35 };
            gridCart.Columns.Add(btnR);
            gridCart.CellClick += (s, e) => { if (e.RowIndex >= 0 && gridCart.Columns[e.ColumnIndex].Name == "Remove") { cartTable.Rows[e.RowIndex].Delete(); UpdateCart(); } };

            Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 55, BackColor = DarkBg };
            lblTotal = new Label { Text = "Итого: 0 ₽", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Success, Location = new Point(panel.Width - 230, 12), Anchor = AnchorStyles.Right, AutoSize = true };

            Button btnClear = new Button { Text = "Очистить", Location = new Point(0, 12), Size = new Size(100, 30), BackColor = Danger, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 100, 30, 6, 6));
            btnClear.Click += (s, e) => { if (MessageBox.Show("Очистить?", "", MessageBoxButtons.YesNo) == DialogResult.Yes) { cartTable.Clear(); UpdateCart(); } };
            bottom.Controls.AddRange(new Control[] { lblTotal, btnClear });

            panel.Controls.AddRange(new Control[] { title, gridCart, bottom });
            panel.Resize += (s, e) => { gridCart.Size = new Size(panel.Width - 2, panel.Height - 120); lblTotal.Location = new Point(panel.Width - 230, 12); };
            page.Controls.Add(panel);

            cartTable = new DataTable();
            cartTable.Columns.Add("Id", typeof(int));
            cartTable.Columns.Add("Название", typeof(string));
            cartTable.Columns.Add("Бренд", typeof(string));
            cartTable.Columns.Add("Размер", typeof(string));
            cartTable.Columns.Add("Кол-во", typeof(int));
            cartTable.Columns.Add("Цена", typeof(string));
            cartTable.Columns.Add("Сумма", typeof(string));
            return page;
        }

        private TabPage CreateUsersTab()
        {
            TabPage page = new TabPage("Пользователи") { BackColor = DarkBg };
            Panel panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), BackColor = DarkBg };

            Label title = new Label { Text = "Пользователи", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = TextPrimary, Location = new Point(0, 5), AutoSize = true };

            gridUsers = CreateGrid();
            gridUsers.Location = new Point(0, 45);
            gridUsers.Size = new Size(panel.Width - 5, panel.Height - 90);
            gridUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            Button btnRefresh = new Button { Text = "Обновить", Location = new Point(0, panel.Height - 35), Size = new Size(120, 28), BackColor = Accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Anchor = AnchorStyles.Bottom };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 120, 28, 6, 6));
            btnRefresh.Click += async (s, e) => await LoadUsers();

            panel.Controls.AddRange(new Control[] { title, gridUsers, btnRefresh });
            panel.Resize += (s, e) => { gridUsers.Size = new Size(panel.Width - 5, panel.Height - 90); btnRefresh.Location = new Point(0, panel.Height - 35); };
            page.Controls.Add(panel);
            return page;
        }

        private void ShowLogin() { loginPanel.Visible = true; mainPanel.Visible = false; this.Show(); }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            lblStatus.Text = "Вход...";
            try
            {
                var r = await client.PostAsJsonAsync("api/users/login", new { login = txtLogin.Text, password = txtPassword.Text });
                if (r.IsSuccessStatusCode)
                {
                    var json = await r.Content.ReadAsStringAsync();
                    using var d = JsonDocument.Parse(json);
                    var root = d.RootElement;
                    currentCustomerId = root.GetProperty("userId").GetInt32();
                    currentCustomerLogin = root.GetProperty("login").GetString();
                    currentUserRole = root.GetProperty("role").GetString();
                    isAdmin = currentUserRole == "admin";
                    OpenMain();
                }
                else { lblStatus.Text = "Неверные данные"; }
            }
            catch { lblStatus.Text = "Сервер недоступен"; }
            btnLogin.Enabled = true;
        }

        private async void BtnRegister_Click(object sender, EventArgs e)
        {
            Form f = new Form { Text = "Регистрация", Size = new Size(320, 360), StartPosition = FormStartPosition.CenterParent, BackColor = CardBg, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
            f.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 320, 360, 10, 10));
            f.Controls.Add(new Label { Text = "Регистрация", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = TextPrimary, Location = new Point(20, 15) });

            string[] lbs = { "Логин", "Пароль", "Имя", "Фамилия", "Телефон", "Email" };
            TextBox[] flds = new TextBox[6];
            for (int i = 0; i < 6; i++)
            {
                flds[i] = new TextBox { Location = new Point(20, 55 + i * 42), Size = new Size(270, 28), BackColor = InputBg, ForeColor = TextSecondary, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 9), Text = lbs[i] };
                flds[i].Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 270, 28, 5, 5));
                if (i == 1) flds[i].PasswordChar = '●';
                int idx = i;
                flds[i].Enter += (s, ev) => { if (flds[idx].Text == lbs[idx]) { flds[idx].Text = ""; flds[idx].ForeColor = TextPrimary; } };
                flds[i].Leave += (s, ev) => { if (string.IsNullOrWhiteSpace(flds[idx].Text)) { flds[idx].Text = lbs[idx]; flds[idx].ForeColor = TextSecondary; flds[idx].PasswordChar = '\0'; } };
                f.Controls.Add(flds[i]);
            }

            Button b = new Button { Text = "Зарегистрироваться", Location = new Point(20, 310), Size = new Size(270, 32), BackColor = Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            b.FlatAppearance.BorderSize = 0;
            b.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 270, 32, 6, 6));
            b.Click += async (s, ev) =>
            {
                if (flds[0].Text == "Логин" || flds[1].Text == "Пароль") { MessageBox.Show("Заполните логин и пароль!"); return; }
                try
                {
                    var r = await client.PostAsJsonAsync("api/users/register", new { login = flds[0].Text, password = flds[1].Text, firstName = flds[2].Text == "Имя" ? "" : flds[2].Text, lastName = flds[3].Text == "Фамилия" ? "" : flds[3].Text, phone = flds[4].Text == "Телефон" ? "" : flds[4].Text, email = flds[5].Text == "Email" ? "" : flds[5].Text });
                    if (r.IsSuccessStatusCode) { MessageBox.Show("Успешно!"); f.Close(); }
                    else MessageBox.Show("Ошибка");
                }
                catch { MessageBox.Show("Сервер недоступен"); }
            };
            f.Controls.Add(b);
            f.ShowDialog();
        }

        private async void BtnGuest_Click(object sender, EventArgs e)
        {
            currentCustomerId = -1; currentCustomerLogin = "Гость"; currentUserRole = "guest"; isAdmin = false;
            OpenMain();
            await LoadProducts();
        }

        private void OpenMain()
        {
            loginPanel.Visible = false; mainPanel.Visible = true;
            lblUser.Text = $"{currentCustomerLogin} ({currentUserRole})";
            tabControl.TabPages.Clear();

            if (isAdmin)
            {
                TabPage catPage = CreateCatalogTab();
                Panel catPanel = (Panel)catPage.Controls[0];

                Panel adminBar = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.FromArgb(22, 22, 22), Padding = new Padding(5, 3, 5, 0) };

                txtProdName = CreateAdminField(adminBar, 0, 160, "Название");
                txtProdBrand = CreateAdminField(adminBar, 165, 110, "Бренд");
                txtProdCategory = CreateAdminField(adminBar, 280, 110, "Категория");
                txtProdColor = CreateAdminField(adminBar, 395, 90, "Цвет");
                numProdPrice = CreateAdminNum(adminBar, 490, 80, 1000);
                numProdStock = CreateAdminNum(adminBar, 575, 65, 10);

                Button btnAdd = new Button { Text = "+", Location = new Point(645, 0), Size = new Size(35, 26), BackColor = Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
                btnAdd.FlatAppearance.BorderSize = 0;
                btnAdd.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 35, 26, 5, 5));
                btnAdd.Click += async (s, ev) => await AddProduct();

                Button btnDel = new Button { Text = "−", Location = new Point(685, 0), Size = new Size(35, 26), BackColor = Danger, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12, FontStyle.Bold), Cursor = Cursors.Hand };
                btnDel.FlatAppearance.BorderSize = 0;
                btnDel.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 35, 26, 5, 5));
                btnDel.Click += async (s, ev) => await DeleteProduct();

                adminBar.Controls.AddRange(new Control[] { txtProdName, txtProdBrand, txtProdCategory, txtProdColor, numProdPrice, numProdStock, btnAdd, btnDel });
                catPanel.Controls.Add(adminBar);

                gridProducts.Dock = DockStyle.None;
                gridProducts.Location = new Point(15, 100);
                gridProducts.Size = new Size(catPanel.Width - 30, catPanel.Height - 115);
                gridProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                catPanel.Resize += (s, ev) => gridProducts.Size = new Size(catPanel.Width - 30, catPanel.Height - 115);

                tabControl.TabPages.Add(catPage);
                tabControl.TabPages.Add(CreateUsersTab());
                cartCountLabel.Visible = false;
            }
            else
            {
                TabPage catPage = CreateCatalogTab();
                DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn { Name = "AddToCart", HeaderText = "", Text = "🛒", UseColumnTextForButtonValue = true, Width = 40 };
                gridProducts.Columns.Add(btnCol);
                gridProducts.CellClick += OnAddToCart;

                tabControl.TabPages.Add(catPage);
                tabControl.TabPages.Add(CreateCartTab());
                UpdateCart();
                cartCountLabel.Visible = true;
            }

            _ = LoadProducts();
            if (isAdmin) _ = LoadUsers();
        }

        private TextBox CreateAdminField(Panel p, int x, int w, string placeholder)
        {
            TextBox tb = new TextBox { Location = new Point(x, 0), Size = new Size(w, 26), BackColor = InputBg, ForeColor = TextSecondary, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 8), Text = placeholder };
            tb.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, w, 26, 4, 4));
            tb.Enter += (s, e) => { if (tb.Text == placeholder) { tb.Text = ""; tb.ForeColor = TextPrimary; } };
            tb.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(tb.Text)) { tb.Text = placeholder; tb.ForeColor = TextSecondary; } };
            return tb;
        }

        private NumericUpDown CreateAdminNum(Panel p, int x, int w, decimal val)
        {
            NumericUpDown n = new NumericUpDown { Location = new Point(x, 0), Size = new Size(w, 26), BackColor = InputBg, ForeColor = TextPrimary, BorderStyle = BorderStyle.None, Font = new Font("Segoe UI", 8), Minimum = 0, Maximum = 100000, Value = val };
            n.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, w, 26, 4, 4));
            return n;
        }

        private async Task LoadProducts()
        {
            try
            {
                string url = isAdmin ? "api/admin/products" : "api/products";
                var r = await client.GetAsync(url);
                if (r.IsSuccessStatusCode)
                {
                    var products = await r.Content.ReadFromJsonAsync<List<JsonElement>>();
                    if (products != null && products.Count > 0)
                    {
                        var dt = new DataTable();
                        dt.Columns.Add("Id", typeof(int));
                        dt.Columns.Add("Название", typeof(string));
                        dt.Columns.Add("Бренд", typeof(string));
                        dt.Columns.Add("Категория", typeof(string));
                        dt.Columns.Add("Цена", typeof(string));
                        dt.Columns.Add("Цвет", typeof(string));
                        dt.Columns.Add("Наличие", typeof(int));

                        foreach (var p in products)
                        {
                            int id = p.TryGetProperty("Id", out var v1) ? v1.GetInt32() : p.GetProperty("id").GetInt32();
                            string name = p.TryGetProperty("Name", out var v2) ? v2.GetString() ?? "" : p.GetProperty("name").GetString() ?? "";
                            string brand = p.TryGetProperty("Brand", out var v3) ? v3.GetString() ?? "" : p.GetProperty("brand").GetString() ?? "";
                            string category = p.TryGetProperty("Category", out var v4) ? v4.GetString() ?? "" : p.GetProperty("category").GetString() ?? "";
                            decimal price = p.TryGetProperty("CurrentPrice", out var v5) ? v5.GetDecimal() : p.GetProperty("currentPrice").GetDecimal();
                            string color = p.TryGetProperty("Color", out var v6) ? v6.GetString() ?? "" : p.GetProperty("color").GetString() ?? "";
                            int stock = p.TryGetProperty("StockQuantity", out var v7) ? v7.GetInt32() : p.GetProperty("stockQuantity").GetInt32();

                            dt.Rows.Add(id, name, brand, category, price.ToString("N0") + " ₽", color, stock);
                        }

                        gridProducts.DataSource = dt;
                        foreach (DataGridViewColumn c in gridProducts.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        lblProductStatus.Text = "Найдено: " + products.Count;
                        lblProductStatus.ForeColor = Success;
                    }
                }
            }
            catch (Exception ex)
            {
                lblProductStatus.Text = "Ошибка: " + ex.Message;
                lblProductStatus.ForeColor = Danger;
            }
        }

        private async Task LoadUsers()
        {
            try
            {
                var r = await client.GetAsync("api/users");
                if (r.IsSuccessStatusCode)
                {
                    var users = await r.Content.ReadFromJsonAsync<List<JsonElement>>();
                    if (users != null && users.Count > 0)
                    {
                        var dt = new DataTable();
                        dt.Columns.Add("ID", typeof(int));
                        dt.Columns.Add("Логин", typeof(string));
                        dt.Columns.Add("Имя", typeof(string));
                        dt.Columns.Add("Фамилия", typeof(string));
                        dt.Columns.Add("Телефон", typeof(string));
                        dt.Columns.Add("Email", typeof(string));
                        dt.Columns.Add("Роль", typeof(string));
                        foreach (var u in users)
                            dt.Rows.Add(GetProp(u, "Id"), GetProp(u, "Login"), GetProp(u, "FirstName"), GetProp(u, "LastName"), GetProp(u, "Phone"), GetProp(u, "Email"), GetProp(u, "Role"));
                        gridUsers.DataSource = dt;
                        foreach (DataGridViewColumn c in gridUsers.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                }
            }
            catch { }
        }

        private string GetProp(JsonElement e, string name)
        {
            if (e.TryGetProperty(name, out var v))
            {
                if (v.ValueKind == JsonValueKind.Number) return v.GetInt32().ToString();
                return v.GetString() ?? "";
            }
            return "";
        }

        private async Task AddProduct()
        {
            string name = txtProdName.Text == "Название" ? "" : txtProdName.Text;
            if (string.IsNullOrWhiteSpace(name)) { MessageBox.Show("Введите название!"); return; }
            try
            {
                await client.PostAsJsonAsync("api/admin/products", new
                {
                    name,
                    brand = txtProdBrand.Text == "Бренд" ? "" : txtProdBrand.Text,
                    category = txtProdCategory.Text == "Категория" ? "" : txtProdCategory.Text,
                    price = numProdPrice.Value,
                    color = txtProdColor.Text == "Цвет" ? "" : txtProdColor.Text,
                    stockQuantity = (int)numProdStock.Value
                });
                txtProdName.Text = "Название"; txtProdName.ForeColor = TextSecondary;
                txtProdBrand.Text = "Бренд"; txtProdBrand.ForeColor = TextSecondary;
                txtProdCategory.Text = "Категория"; txtProdCategory.ForeColor = TextSecondary;
                txtProdColor.Text = "Цвет"; txtProdColor.ForeColor = TextSecondary;
                numProdPrice.Value = 1000; numProdStock.Value = 10;
                await LoadProducts();
            }
            catch { MessageBox.Show("Ошибка!"); }
        }

        private async Task DeleteProduct()
        {
            if (gridProducts.SelectedRows.Count == 0) { MessageBox.Show("Выберите товар!"); return; }
            if (MessageBox.Show("Удалить?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    int id = Convert.ToInt32(gridProducts.SelectedRows[0].Cells["Id"].Value);
                    await client.DeleteAsync($"api/admin/products/{id}");
                    await LoadProducts();
                }
                catch { MessageBox.Show("Ошибка!"); }
            }
        }

        private void OnAddToCart(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || gridProducts.Columns[e.ColumnIndex].Name != "AddToCart") return;
            var row = gridProducts.Rows[e.RowIndex];
            int id = Convert.ToInt32(row.Cells["Id"].Value);
            string name = row.Cells["Название"].Value?.ToString() ?? "";
            string brand = row.Cells["Бренд"].Value?.ToString() ?? "";
            string ps = row.Cells["Цена"].Value?.ToString()?.Replace(" ₽", "").Replace(" ", "") ?? "0";
            decimal price = decimal.Parse(ps);

            Form f = new Form { Text = "Размер", Size = new Size(230, 140), StartPosition = FormStartPosition.CenterParent, BackColor = CardBg, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false };
            f.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 230, 140, 8, 8));

            ComboBox cb = new ComboBox { Location = new Point(15, 15), Size = new Size(200, 24), BackColor = InputBg, ForeColor = TextPrimary, DropDownStyle = ComboBoxStyle.DropDownList };
            cb.Items.AddRange(new[] { "36", "37", "38", "39", "40", "41", "42", "43", "44", "45" }); cb.SelectedIndex = 4;

            NumericUpDown nq = new NumericUpDown { Location = new Point(15, 50), Size = new Size(200, 24), BackColor = InputBg, ForeColor = TextPrimary, Minimum = 1, Maximum = 10, Value = 1 };

            Button ba = new Button { Text = "Добавить", Location = new Point(15, 85), Size = new Size(200, 28), BackColor = Success, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            ba.FlatAppearance.BorderSize = 0;
            ba.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, 200, 28, 5, 5));
            ba.Click += (s, ev) =>
            {
                string sz = cb.SelectedItem?.ToString() ?? "40"; int qt = (int)nq.Value;
                var ex = cartTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["Id"]) == id && r["Размер"].ToString() == sz);
                if (ex != null) { ex["Кол-во"] = Convert.ToInt32(ex["Кол-во"]) + qt; ex["Сумма"] = (price * Convert.ToInt32(ex["Кол-во"])).ToString("N0") + " ₽"; }
                else cartTable.Rows.Add(id, name, brand, sz, qt, price.ToString("N0") + " ₽", (price * qt).ToString("N0") + " ₽");
                UpdateCart(); f.Close();
            };

            f.Controls.AddRange(new Control[] { cb, nq, ba }); f.ShowDialog();
        }

        private void UpdateCart()
        {
            cartTable.AcceptChanges(); gridCart.DataSource = null; gridCart.DataSource = cartTable;
            if (gridCart.Columns.Contains("Id")) gridCart.Columns["Id"].Visible = false;
            foreach (DataGridViewColumn c in gridCart.Columns) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            decimal total = 0;
            foreach (DataRow row in cartTable.Rows) { string s = row["Сумма"].ToString()?.Replace(" ₽", "").Replace(" ", "") ?? "0"; total += decimal.Parse(s); }
            lblTotal.Text = "Итого: " + total.ToString("N0") + " ₽";
            cartCountLabel.Text = "Корзина (" + cartTable.Rows.Count + ")";
        }
    }
}