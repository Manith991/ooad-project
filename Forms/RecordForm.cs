using System;
using System.Windows.Forms;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project
{
    /// <summary>
    /// RecordForm – uses:
    ///   REPOSITORY PATTERN : OrderRepository handles all DB access
    ///
    /// Note: Order deletion is NOT wrapped in the Command pattern deliberately.
    /// Financial records should not be silently un-deleted to protect audit integrity.
    /// </summary>
    public partial class RecordForm : Form
    {
        // ✅ REPOSITORY PATTERN
        private readonly OrderRepository _repo;

        private readonly string currentRole;

        public RecordForm(string role)
        {
            InitializeComponent();
            currentRole = role;

            // ✅ REPOSITORY PATTERN
            _repo = new OrderRepository();

            dgvRecord.Columns["colDelete"].Visible =
                currentRole.Equals("(admin)", StringComparison.OrdinalIgnoreCase);

            LoadRecords();
        }

        // ─── Load ───────────────────────────────────────────────────────────
        private void LoadRecords()
        {
            dgvRecord.Rows.Clear();

            // ✅ REPOSITORY PATTERN: no inline SQL, single call
            var orders = _repo.GetAll();
            int i = 0;

            foreach (var order in orders)
            {
                i++;
                int rowIndex = dgvRecord.Rows.Add();
                var row = dgvRecord.Rows[rowIndex];

                row.Cells["colNo"].Value = i;
                row.Cells["colTable"].Value = order.TableName ?? "-";
                row.Cells["colStaff"].Value = order.StaffName ?? "-";
                row.Cells["colOrder"].Value = order.OrderType;
                row.Cells["colDate"].Value = order.OrderDate.ToString("yyyy-MM-dd HH:mm");
                row.Cells["colTotal"].Value = order.TotalAmount.ToString("C");
                row.Cells["colStatus"].Value = order.Status;
                row.Cells["colPayment"].Value = order.PaymentMethod ?? "-";

                row.Cells["colDetail"].Value = Properties.Resources.detail;
                row.Cells["colDelete"].Value = Properties.Resources.delete;

                row.Tag = order.OrderId;   // ✅ store clean int, not boxed object
            }
        }

        // ─── Cell click: Detail / Delete ────────────────────────────────────
        private void dgvRecord_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string colName = dgvRecord.Columns[e.ColumnIndex].Name;
            var row = dgvRecord.Rows[e.RowIndex];

            if (!TryGetOrderId(row, out int orderId)) return;

            if (colName == "colDetail")
            {
                try
                {
                    var detailForm = new FormDetailRecord(orderId);
                    detailForm.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening detail: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (colName == "colDelete")
            {
                if (!currentRole.Equals("(admin)", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Only admins can delete records.", "Access Denied",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete order #{orderId}?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                try
                {
                    // ✅ REPOSITORY PATTERN: delete via repository
                    _repo.Delete(orderId);
                    MessageBox.Show("Order deleted successfully.", "Deleted",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadRecords();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting order: " + ex.Message,
                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ─── Helper ──────────────────────────────────────────────────────────
        private bool TryGetOrderId(DataGridViewRow row, out int orderId)
        {
            orderId = -1;
            try
            {
                if (row.Tag is int id) { orderId = id; return orderId > 0; }
                if (row.Tag is not null) { orderId = Convert.ToInt32(row.Tag); return orderId > 0; }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading order id: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
    }
}