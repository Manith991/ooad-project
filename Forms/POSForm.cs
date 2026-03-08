using Npgsql;
using OOAD_Project.Components;
using OOAD_Project.Domain;
using OOAD_Project.Patterns;
using OOAD_Project.Patterns.Command;
using OOAD_Project.Patterns.Repository;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OOAD_Project
{
    public partial class POSForm : Form
    {
        #region WinAPI for Dragging Form
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        #endregion

        // ✅ STRATEGY PATTERN - Payment processing
        private readonly PaymentContext _paymentContext;

        // ✅ OBSERVER PATTERN - Order state management
        private ObservableOrder? _observableOrder;

        // ✅ REPOSITORY PATTERN - Centralized data access (no raw SQL in form)
        private readonly OrderRepository _orderRepo;
        private readonly OrderItemRepository _orderItemRepo;
        private readonly ProductRepository _productRepo;
        private readonly UserRepository _userRepo;
        private readonly TableRepository _tableRepo;

        private readonly string currentUser;
        private readonly string orderType;
        private readonly string? tableName;
        private readonly int? tableId;
        private int? orderId;
        private readonly Form? parentForm;

        // ✅ COMMAND PATTERN - Undo / Redo
        private readonly CartCommandManager _cmdManager = new();
        private readonly List<CartItem> _cart = new();
        public POSForm(Form? parent, string username, string orderType, string? tableName = null, int? tableId = null, int? orderId = null)
        {
            InitializeComponent();
            SetupDataGridView();

            AssignCategoryTags();

            parentForm = parent;
            currentUser = username;
            this.orderType = orderType;
            this.tableName = tableName;
            this.tableId = tableId;
            this.orderId = orderId;

            this.KeyPreview = true;
            this.KeyDown += POSForm_KeyDown;

            // ✅ Initialize Strategy Pattern
            _paymentContext = new PaymentContext();

            // ✅ Initialize Repositories (no more raw SQL scattered in form)
            _orderRepo = new OrderRepository();
            _orderItemRepo = new OrderItemRepository();
            _productRepo = new ProductRepository();
            _userRepo = new UserRepository();
            _tableRepo = new TableRepository();

            InitializeOrderDetails();
        }

        public POSForm() { }

        #region Initialization

        private void InitializeOrderDetails()
        {
            lblUser.Text = currentUser;
            lblOrderType.Text = orderType;

            lblTableName.Visible = orderType.Equals("Dine-in", StringComparison.OrdinalIgnoreCase)
                                || orderType.Equals("Eat Here", StringComparison.OrdinalIgnoreCase);
            lblTableName.Text = tableName ?? "Unknown Table";

            if (orderId.HasValue)
            {
                LoadExistingOrderDetails();
            }

            // ✅ OBSERVER PATTERN: Always set up observers (existing OR new order)
            SetupOrderObservers();
        }

        // ============================================
        // OBSERVER PATTERN: Setup observers
        // Works for both new and existing orders
        // ============================================
        private void SetupOrderObservers()
        {
            var order = new Order
            {
                OrderId = orderId ?? 0,
                TableId = tableId,
                UserId = GetCurrentUserId(),
                OrderType = orderType,
                TotalAmount = 0,
                Status = "Unpaid"
            };

            _observableOrder = new ObservableOrder(order);

            _observableOrder.Attach(new TableStatusObserver(parentForm as DiningForm));
            _observableOrder.Attach(new OrderLoggingObserver());
            _observableOrder.Attach(new SalesStatisticsObserver());
            _observableOrder.Attach(new NotificationObserver());

            Console.WriteLine($"[POSForm] Observers attached to Order #{orderId?.ToString() ?? "NEW"}");
        }

        private void POSForm_Shown(object sender, EventArgs e)
        {
            LoadProductsFromDatabase();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            this.WindowState = FormWindowState.Maximized;
        }

        #endregion

        #region Load Products

        private void LoadProductsFromDatabase(string? categoryFilter = null)
        {
            flowLayoutPanel.Controls.Clear();
            flowLayoutPanel.Padding = new Padding(20);

            IEnumerable<Product> products = string.IsNullOrEmpty(categoryFilter) || categoryFilter == "All Foods"
                ? _productRepo.GetAll()
                : _productRepo.GetByCategory(categoryFilter);

            var productList = products.ToList();
            if (productList.Count == 0)
            {
                flowLayoutPanel.Controls.Add(CreateNoProductLabel());
                return;
            }

            foreach (var product in productList)
            {
                flowLayoutPanel.Controls.Add(CreateFoodCard(
                    product.ProductName,
                    (double)product.Price,
                    product.ImagePath ?? ""
                ));
            }
        }

        private Label CreateNoProductLabel()
        {
            return new Label
            {
                Text = "No products found in this category.",
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12, FontStyle.Italic),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };
        }

        private FoodCard CreateFoodCard(string name, double price, string imagePath)
        {
            var card = new FoodCard
            {
                Title = name,
                Price = price,
                Food = LoadImage(imagePath),
                Margin = new Padding(10)
            };
            card.OnSelect += FoodCard_OnSelect;
            return card;
        }

        private Image? LoadImage(string path)
        {
            string[] pathsToCheck =
            {
                path,
                Path.Combine(Application.StartupPath, "Resources", "Foods", path),
                Path.Combine(Application.StartupPath, "Resources", "Foods", path + ".png"),
                Path.Combine(Application.StartupPath, "Resources", "Foods", path + ".jpg"),
                Path.Combine(Application.StartupPath, "Resources", "Foods", path + ".jpeg")
            };

            foreach (var p in pathsToCheck)
                if (File.Exists(p)) return Image.FromFile(p);

            string defaultImg = Path.Combine(Application.StartupPath, "Resources", "no_image.png");
            return File.Exists(defaultImg) ? Image.FromFile(defaultImg) : null;
        }

        #endregion

        #region Search

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search items...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search items...";
                txtSearch.ForeColor = Color.Gray;
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Search items...") return;
            foreach (FoodCard card in flowLayoutPanel.Controls)
                card.Visible = card.Title.IndexOf(txtSearch.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        #endregion

        #region DataGridView Helpers

        private void SetupDataGridView()
        {
            dgvItems.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvItems.AdvancedCellBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.Single;
            dgvItems.AdvancedCellBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            dgvItems.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dgvItems.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;

            dgvItems.RowsAdded += (s, e) => UpdateRowNumbers();
            dgvItems.RowsRemoved += (s, e) => UpdateRowNumbers();

            if (dgvItems.Columns["no"] != null)
                dgvItems.Columns["no"].ReadOnly = true;
        }

        private void UpdateRowNumbers()
        {
            for (int i = 0; i < dgvItems.Rows.Count; i++)
                dgvItems.Rows[i].Cells["no"].Value = (i + 1).ToString();
        }

        private void FoodCard_OnSelect(object? sender, EventArgs e)
        {
            if (sender is not FoodCard card) return;

            var product = _productRepo.GetAll()
                .FirstOrDefault(p => p.ProductName == card.Title);

            if (product == null)
            {
                MessageBox.Show("Product not found in database.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var customDialog = new FormProductCustomization(
                product.ProductId,
                product.ProductName,
                product.Price,
                product.CategoryName ?? "Main Dish");

            if (customDialog.ShowDialog() != DialogResult.OK) return;

            IProduct customizedProduct = customDialog.CustomizedProduct;
            string itemName = customizedProduct.GetDescription();
            decimal itemPrice = customizedProduct.GetPrice();

            EnsureOrderExists();

            // ✅ COMMAND PATTERN: wrap add in undoable command
            var cartItem = new CartItem
            {
                ProductId = product.ProductId,
                ProductName = itemName,
                Price = itemPrice,
                Quantity = 1
            };

            _cmdManager.Execute(new AddCartItemCommand(_cart, cartItem));
            RebuildGridFromCart();

            // Sync new state to DB
            SyncCartToDb();
            UpdateGrandTotal();
        }

        private void dgvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvItems.Columns[e.ColumnIndex].Name != "quantity") return;

            var row = dgvItems.Rows[e.RowIndex];
            if (row.Tag is not int productId) return;

            if (int.TryParse(row.Cells["quantity"].Value?.ToString(), out int newQty) && newQty > 0)
            {
                _cmdManager.Execute(new UpdateCartQuantityCommand(_cart, productId, newQty));
                RebuildGridFromCart();
                SyncCartToDb();
            }
            else
            {
                // Revert invalid input
                RebuildGridFromCart();
            }

            UpdateGrandTotal();
        }

        private void dgvItems_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvItems.CurrentCell.ColumnIndex == dgvItems.Columns["quantity"].Index)
            {
                e.Control.KeyPress -= Quantity_KeyPress;
                e.Control.KeyPress += Quantity_KeyPress;
            }
        }

        private void Quantity_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        #endregion

        #region Remove / Clear

        // ✅ FIX: Now deletes from DB so items don't reappear on reopen
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (dgvItems.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item to remove.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            foreach (DataGridViewRow row in dgvItems.SelectedRows.Cast<DataGridViewRow>().ToList())
            {
                if (row.Tag is int productId)
                {
                    _cmdManager.Execute(new RemoveCartItemCommand(_cart, productId));
                }
            }

            RebuildGridFromCart();
            SyncCartToDb();
            UpdateGrandTotal();
        }

        // ✅ FIX: Now clears all items from DB so they don't reappear on reopen
        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("There are no items to clear.", "Empty List",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show("Are you sure you want to clear all items?", "Confirm Clear All",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            _cmdManager.Execute(new ClearCartCommand(_cart));
            RebuildGridFromCart();
            SyncCartToDb();
            UpdateGrandTotal();
        }

        /// <summary>Deletes a single order_detail row from the database.</summary>
        private void DeleteOrderDetailFromDb(int orderId, int productId)
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "DELETE FROM order_details WHERE order_id = @oid AND product_id = @pid;", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.Parameters.AddWithValue("@pid", productId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POSForm] DeleteOrderDetail error: {ex.Message}");
            }
        }

        /// <summary>Deletes ALL order_detail rows for the given order from the database.</summary>
        private void DeleteAllOrderDetailsFromDb(int orderId)
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "DELETE FROM order_details WHERE order_id = @oid;", conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POSForm] DeleteAllOrderDetails error: {ex.Message}");
            }
        }

        #endregion

        #region Payments

        private void btnPay_Click(object sender, EventArgs e)
        {
            _paymentContext.SetStrategy(new CashPaymentStrategy());
            ProcessPayment();
        }

        private void btnPayQR_Click(object sender, EventArgs e)
        {
            _paymentContext.SetStrategy(new QRPaymentStrategy());
            ProcessPayment();
        }

        // ✅ STRATEGY PATTERN: Unified payment processing
        private void ProcessPayment()
        {
            double total = dgvItems.Rows.Cast<DataGridViewRow>()
                .Where(r => r.Cells["total"].Value != null)
                .Sum(r => Convert.ToDouble(r.Cells["total"].Value));

            if (total <= 0)
            {
                MessageBox.Show("No items to pay for.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var order = _observableOrder?.GetOrder() ?? new Order
            {
                OrderId = orderId ?? 0,
                TableId = tableId,
                UserId = GetCurrentUserId(),
                OrderType = orderType,
                TotalAmount = (decimal)total,
                Status = "Unpaid"
            };

            // ✅ Execute payment via Strategy
            bool success = _paymentContext.ExecutePayment((decimal)total, order);

            if (success)
            {
                CompletePaymentInDatabase(total);

                // ✅ OBSERVER PATTERN: Notify all observers of payment
                if (_observableOrder != null)
                {
                    _observableOrder.UpdateTotal((decimal)total);
                    _observableOrder.UpdateStatus("Paid");
                }

                dgvItems.Rows.Clear();
                UpdateGrandTotal();
                Close();
            }
        }

        // ✅ Uses Repositories instead of raw SQL
        private void CompletePaymentInDatabase(double total)
        {
            try
            {
                if (!orderId.HasValue) return;

                // ✅ Repository: update order status and total
                var order = _orderRepo.GetById(orderId.Value);
                if (order != null)
                {
                    order.TotalAmount = (decimal)total;
                    order.Status = "Paid";
                    order.PaymentMethod = _paymentContext.GetCurrentMethod();
                    _orderRepo.Update(order);
                }

                // ✅ Repository: set table available if dine-in
                if (tableId.HasValue)
                    _tableRepo.UpdateStatus(tableId.Value, "Available");

                MessageBox.Show($"{_paymentContext.GetCurrentMethod()} Payment successful.",
                    "Paid", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Payment error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateGrandTotal()
        {
            double sum = dgvItems.Rows.Cast<DataGridViewRow>()
                .Where(r => r.Cells["total"].Value != null)
                .Sum(r => Convert.ToDouble(r.Cells["total"].Value));

            lbTotalPrice.Text = sum.ToString("C2");

            // ✅ OBSERVER PATTERN: Notify observers of total change
            _observableOrder?.UpdateTotal((decimal)sum);
        }

        #endregion

        #region Categories

        private void btnAllFood_Click(object sender, EventArgs e) => LoadProductsFromDatabase("All Foods");
        private void btnMainDish_Click(object sender, EventArgs e) => LoadProductsFromDatabase("Main Dishes");
        private void btnAppetizer_Click(object sender, EventArgs e) => LoadProductsFromDatabase("Appetizers");
        private void btnSideDish_Click(object sender, EventArgs e) => LoadProductsFromDatabase("Side Dishes");
        private void btnSoup_Click(object sender, EventArgs e) => LoadProductsFromDatabase("Soups/Salads");
        private void btnSeafood_Click(object sender, EventArgs e) => LoadProductsFromDatabase("Seafood");
        private void btnBeverage_Click(object sender, EventArgs e) => LoadProductsFromDatabase("Beverages");
        private void btnDessert_Click(object sender, EventArgs e) => LoadProductsFromDatabase("Desserts");

        private void AssignCategoryTags()
        {
            string query = "SELECT categoryid, categoryname FROM categories ORDER BY categoryid ASC";
            DataTable dt = Database.GetData(query);

            int i = 0;
            var buttons = new[]
            {
                btnAllFood, btnMainDish, btnAppetizer, btnSideDish,
                btnSoup, btnSeafood, btnBeverage, btnDessert
            };

            foreach (DataRow row in dt.Rows)
            {
                if (i >= buttons.Length) break;
                buttons[i].Tag = Convert.ToInt32(row["categoryid"]);
                buttons[i].Text = row["categoryname"].ToString();
                i++;
            }
        }

        public void RefreshCategoryButtons()
        {
            string query = "SELECT categoryid, categoryname, imagepath FROM categories";
            DataTable dt = Database.GetData(query);

            foreach (DataRow row in dt.Rows)
            {
                int id = Convert.ToInt32(row["categoryid"]);
                string name = row["categoryname"].ToString();
                string imgPath = row["imagepath"].ToString();

                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is Guna.UI2.WinForms.Guna2Button btn && btn.Tag is int tagId && tagId == id)
                    {
                        btn.Text = name;
                        string fullPath = Path.Combine(Application.StartupPath, "Resources", imgPath);
                        if (File.Exists(fullPath))
                        {
                            btn.Image?.Dispose();
                            btn.Image = Image.FromFile(fullPath);
                        }
                    }
                }
            }
        }

        private void btnManageCategories_Click(object sender, EventArgs e)
        {
            using var form = new CategoriesForm("admin");
            if (form.ShowDialog() == DialogResult.OK)
                RefreshCategoryButtons();
        }

        #endregion

        #region Misc

        private void panelTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Close();
            parentForm?.Show();
        }

        #endregion

        #region Load Existing Order

        // ✅ Uses OrderItemRepository instead of raw SQL
        #region Load Existing Order

        private void LoadExistingOrderDetails()
        {
            if (!orderId.HasValue) return;

            var items = _orderItemRepo.GetByOrderId(orderId.Value);

            // ✅ Populate _cart so undo/redo works on existing orders too
            _cart.Clear();
            _cmdManager.Clear(); // fresh history — no undoing pre-loaded items

            foreach (var item in items)
            {
                _cart.Add(new CartItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity
                });
            }

            RebuildGridFromCart();
            UpdateGrandTotal();
        }

        #endregion

        #endregion

        #region Helpers

        /// <summary>
        /// Creates an order record in the DB if one doesn't exist yet (for new orders).
        /// </summary>
        private void EnsureOrderExists()
        {
            if (orderId.HasValue) return;

            try
            {
                int userId = GetCurrentUserId();
                if (userId == -1) return;

                var newOrder = new Order
                {
                    TableId = tableId,
                    UserId = userId,
                    OrderType = orderType ?? "Takeaway",
                    OrderDate = DateTime.Now,
                    TotalAmount = 0,
                    Status = "Pending",
                    PaymentMethod = "Cash"
                };

                orderId = _orderRepo.Add(newOrder);

                // ✅ OBSERVER: Update the observable order now that we have an ID
                if (_observableOrder != null)
                {
                    var order = _observableOrder.GetOrder();
                    order.OrderId = orderId.Value;
                }

                Console.WriteLine($"[POSForm] Created new order #{orderId}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating order: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets the current user's ID via UserRepository.
        /// </summary>
        private int GetCurrentUserId()
        {
            var user = _userRepo.GetByUsername(currentUser);
            if (user == null)
            {
                MessageBox.Show("User not found: " + currentUser, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            return user.Id;
        }

        #endregion

        private void POSForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                if (!_cmdManager.CanUndo)
                {
                    MessageBox.Show("Nothing to undo.", "Undo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Handled = true;
                    return;
                }
                _cmdManager.Undo();
                RebuildGridFromCart();
                SyncCartToDb();        // ✅ push restored cart back to DB
                UpdateGrandTotal();
                e.Handled = true;
            }
            else if (e.Control && (e.KeyCode == Keys.Y || (e.Shift && e.KeyCode == Keys.Z)))
            {
                if (!_cmdManager.CanRedo)
                {
                    MessageBox.Show("Nothing to redo.", "Redo",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Handled = true;
                    return;
                }
                _cmdManager.Redo();
                RebuildGridFromCart();
                SyncCartToDb();        // ✅ push re-applied cart back to DB
                UpdateGrandTotal();
                e.Handled = true;
            }
        }
        /// <summary>Rebuilds the DataGridView entirely from the in-memory _cart list.</summary>
        private void RebuildGridFromCart()
        {
            dgvItems.Rows.Clear();

            foreach (var item in _cart)
            {
                int idx = dgvItems.Rows.Add();
                var row = dgvItems.Rows[idx];
                row.Cells["item"].Value = item.ProductName;
                row.Cells["price"].Value = (double)item.Price;
                row.Cells["quantity"].Value = item.Quantity;
                row.Cells["total"].Value = (double)item.Subtotal;
                row.Tag = item.ProductId;
            }

            UpdateRowNumbers();
        }

        /// <summary>
        /// Pushes the current _cart state to the database (full replace of order_details).
        /// </summary>
        private void SyncCartToDb()
        {
            if (!orderId.HasValue) return;

            try
            {
                // Clear existing DB rows for this order, then re-insert from _cart
                DeleteAllOrderDetailsFromDb(orderId.Value);

                foreach (var item in _cart)
                    _orderItemRepo.Upsert(orderId.Value, item.ProductId, item.Quantity, item.Price);

                _orderItemRepo.RefreshOrderTotal(orderId.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POSForm] SyncCartToDb error: {ex.Message}");
            }
        }
    }
}