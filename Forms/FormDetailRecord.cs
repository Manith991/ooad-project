using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project
{
    /// <summary>
    /// REPOSITORY PATTERN applied:
    ///   - Order header loaded via OrderRepository.GetById()
    ///   - Order items loaded via OrderItemRepository.GetByOrderId()
    ///     instead of raw NpgsqlCommand calls inside the form.
    ///
    /// No Command or Observer pattern is needed here — this is a
    /// read-only detail view with no mutations.
    /// </summary>
    public partial class FormDetailRecord : Form
    {
        private readonly int _orderId;

        // REPOSITORY PATTERN – injected or created locally
        private readonly IRepository<Order> _orderRepository;
        private readonly OrderItemRepository _orderItemRepository;

        public FormDetailRecord(
            int orderId,
            IRepository<Order>? orderRepository = null,
            OrderItemRepository? orderItemRepository = null)
        {
            InitializeComponent();

            _orderId = orderId;
            this.StartPosition = FormStartPosition.CenterScreen;

            _orderRepository = orderRepository ?? new OrderRepository();
            _orderItemRepository = orderItemRepository ?? new OrderItemRepository();

            LoadOrderDetail();
            LoadOrderProducts();
        }

        // ── Load order header via Repository ──────────────────────────────
        private void LoadOrderDetail()
        {
            try
            {
                // REPOSITORY PATTERN: single call replaces the raw SQL block
                Order? order = _orderRepository.GetById(_orderId);

                if (order == null)
                {
                    MessageBox.Show("Order not found.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Close();
                    return;
                }

                string status = order.Status ?? "—";

                lblID.Text = "#" + order.OrderId;
                lblTable.Text = order.TableName ?? "—";
                lblStaff.Text = order.StaffName ?? "—";
                lblType.Text = order.OrderType ?? "—";
                lblDate.Text = order.OrderDate.ToString("dd MMM yyyy  HH:mm");
                lblTotal.Text = order.TotalAmount.ToString("C");
                lblStatus.Text = status;

                lblStatus.ForeColor = status.ToLower() switch
                {
                    "completed" => Color.FromArgb(80, 220, 135),
                    "pending" => Color.FromArgb(255, 195, 80),
                    "cancelled" => Color.FromArgb(255, 100, 100),
                    _ => Color.FromArgb(215, 240, 225)
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading order: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Load order items via Repository ───────────────────────────────
        private void LoadOrderProducts()
        {
            try
            {
                // REPOSITORY PATTERN: dedicated repository for order items
                var items = _orderItemRepository.GetByOrderId(_orderId).ToList();

                flowPanelProducts.Controls.Clear();

                if (items.Count == 0)
                {
                    flowPanelProducts.Controls.Add(new Label
                    {
                        Text = "No items found for this order.",
                        AutoSize = true,
                        Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                        Padding = new Padding(18, 14, 0, 0),
                        ForeColor = Color.FromArgb(170, 170, 170)
                    });
                    return;
                }

                foreach (var item in items)
                    flowPanelProducts.Controls.Add(
                        CreateProductRow(item.ProductName, item.Quantity, item.Price, item.Subtotal));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── UI helper: one table row per order item ───────────────────────
        private Panel CreateProductRow(string name, int qty, decimal price, decimal subtotal)
        {
            bool odd = flowPanelProducts.Controls.Count % 2 == 0;

            var row = new Panel
            {
                Width = flowPanelProducts.ClientSize.Width - 2,
                Height = 44,
                BackColor = odd ? Color.FromArgb(252, 252, 250)
                                : Color.FromArgb(245, 249, 246),
                Margin = Padding.Empty,
                Padding = new Padding(0)
            };

            var line = new Panel
            {
                BackColor = Color.FromArgb(228, 234, 228),
                Dock = DockStyle.Bottom,
                Height = 1
            };

            var lblName = new Label
            {
                AutoSize = false,
                Width = 188,
                Height = 44,
                Location = new Point(12, 0),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 45, 35),
                Text = name,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblQty = new Label
            {
                AutoSize = false,
                Width = 50,
                Height = 44,
                Location = new Point(232, 0),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(80, 100, 85),
                Text = qty.ToString(),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblPrice = new Label
            {
                AutoSize = false,
                Width = 90,
                Height = 44,
                Location = new Point(342, 0),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(80, 100, 85),
                Text = price.ToString("C"),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblSub = new Label
            {
                AutoSize = false,
                Width = 90,
                Height = 44,
                Location = new Point(482, 0),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(28, 56, 42),
                Text = subtotal.ToString("C"),
                TextAlign = ContentAlignment.MiddleLeft
            };

            row.Controls.AddRange(new Control[] { line, lblName, lblQty, lblPrice, lblSub });
            return row;
        }

        private void btnClose_Click(object sender, EventArgs e) => Close();
        private void guna2ImageButton1_Click(object sender, EventArgs e) => Close();
        private void lblColPrice_Click(object sender, EventArgs e) { }
    }
}