using System.Runtime.InteropServices;
using OOAD_Project.Components;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project
{
    /// <summary>
    /// REPOSITORY PATTERN applied:
    ///   - Table list loaded via TableRepository instead of inline SQL.
    ///   - User ID lookup via UserRepository instead of inline SQL.
    ///   - Order creation delegated to OrderRepository instead of inline SQL.
    ///
    /// OBSERVER PATTERN (already present in POSForm):
    ///   - DiningForm is passed to POSForm so TableStatusObserver can
    ///     call back and refresh cards when an order is paid.
    ///
    /// No Command pattern needed here – table-card clicks open POSForm
    /// which owns the Command / Strategy / Observer stacks.
    /// </summary>
    public partial class DiningForm : Form
    {
        [DllImport("user32.dll")] public static extern bool ReleaseCapture();
        [DllImport("user32.dll")] public static extern bool SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);

        private readonly string _currentUser;

        // REPOSITORY PATTERN
        private readonly IRepository<Table> _tableRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly UserRepository _userRepository;

        public DiningForm(
            string username,
            IRepository<Table>? tableRepository = null,
            IRepository<Order>? orderRepository = null,
            UserRepository? userRepository = null)
        {
            InitializeComponent();
            _currentUser = username;
            _tableRepository = tableRepository ?? new TableRepository();
            _orderRepository = orderRepository ?? new OrderRepository();
            _userRepository = userRepository ?? new UserRepository();

            LoadTableCards();
        }

        // ── Load table cards via Repository Pattern ───────────────────────
        public void LoadTableCards()
        {
            flpTable.Controls.Clear();
            flpTable.Padding = new Padding(20);

            // REPOSITORY PATTERN: single call replaces raw SQL
            var tables = _tableRepository.GetAll();

            foreach (var table in tables)
            {
                var card = new TableCard();
                card.Margin = new Padding(10);
                card.SetTableData(table.TableId, table.TableName, table.Capacity, table.Status);

                card.Click += (s, e) => HandleTableCardClick(card);

                flpTable.Controls.Add(card);
            }
        }

        // ── Handle a card click ───────────────────────────────────────────
        private void HandleTableCardClick(TableCard card)
        {
            if (card.TableStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
            {
                HandleAvailableTable(card);
            }
            else if (card.TableStatus.Equals("Taken", StringComparison.OrdinalIgnoreCase))
            {
                HandleTakenTable(card);
            }
            else
            {
                MessageBox.Show($"This table is {card.TableStatus}.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ── Case 1: Available → create new order ──────────────────────────
        private void HandleAvailableTable(TableCard card)
        {
            var result = MessageBox.Show(
                $"Do you want to create a new order for {card.TableName}?",
                "New Order", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                // REPOSITORY PATTERN: get user id via repository
                int userId = GetCurrentUserId();
                if (userId == -1)
                {
                    MessageBox.Show("User not found in database.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // REPOSITORY PATTERN: create order via repository
                var newOrder = new Order
                {
                    TableId = card.TableId,
                    UserId = userId,
                    OrderType = "Dine-in",
                    OrderDate = DateTime.Now,
                    TotalAmount = 0,
                    Status = "Unpaid",
                    PaymentMethod = null
                };
                int orderId = _orderRepository.Add(newOrder);

                // REPOSITORY PATTERN: update table status via repository
                ((TableRepository)_tableRepository).UpdateStatus(card.TableId, "Taken");
                card.SetTableData(card.TableId, card.TableName, card.TableCapacity, "Taken");

                var pos = new POSForm(this, _currentUser, "Eat Here",
                    card.TableName, card.TableId, orderId);
                pos.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating order: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Case 2: Taken → open existing unpaid order ────────────────────
        private void HandleTakenTable(TableCard card)
        {
            try
            {
                // REPOSITORY PATTERN: find unpaid order via repository
                var unpaidOrder = ((OrderRepository)_orderRepository)
                    .GetUnpaidOrderForTable(card.TableId);

                if (unpaidOrder != null)
                {
                    var pos = new POSForm(this, _currentUser, "Eat Here",
                        card.TableName, card.TableId, unpaidOrder.OrderId);
                    pos.Show();
                }
                else
                {
                    MessageBox.Show("No unpaid order found for this table.", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading existing order: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helper: current user id via Repository ────────────────────────
        private int GetCurrentUserId()
        {
            // REPOSITORY PATTERN: lookup via repository method
            User? user = _userRepository.GetByUsername(_currentUser);
            return user?.Id ?? -1;
        }
    }
}