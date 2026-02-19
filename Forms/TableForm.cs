using System;
using System.Windows.Forms;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;
using OOAD_Project.Patterns.Command;

namespace OOAD_Project
{
    /// <summary>
    /// TableForm – uses:
    ///   REPOSITORY PATTERN : TableRepository handles all DB access
    ///   COMMAND PATTERN    : Add / Edit / Delete are undoable ICommands
    /// </summary>
    public partial class TableForm : Form
    {
        private readonly record struct TableRowInfo(int TableId);

        // ✅ REPOSITORY PATTERN
        private readonly IRepository<Table> _repo;

        // ✅ COMMAND PATTERN
        private readonly CommandInvoker _invoker = new CommandInvoker();

        private readonly string userRole;

        public TableForm(string userRole)
        {
            InitializeComponent();
            this.userRole = userRole;

            // ✅ REPOSITORY PATTERN
            _repo = new TableRepository();

            LoadTables();
            RestrictActionsByRole();
        }

        // ─── Role restriction ───────────────────────────────────────────────
        private void RestrictActionsByRole()
        {
            if (userRole != "(admin)")
            {
                btnAdd.Enabled = false;
                if (dgvTable.Columns.Contains("colEdit")) dgvTable.Columns["colEdit"].Visible = false;
                if (dgvTable.Columns.Contains("colDelete")) dgvTable.Columns["colDelete"].Visible = false;
            }
        }

        // ─── Load ───────────────────────────────────────────────────────────
        private void LoadTables()
        {
            dgvTable.Rows.Clear();

            // ✅ REPOSITORY PATTERN
            var tables = _repo.GetAll();
            int rowNo = 1;

            foreach (var t in tables)
            {
                int rowIndex = dgvTable.Rows.Add(rowNo++, t.TableName, t.Capacity, t.Status);
                dgvTable.Rows[rowIndex].Tag = new TableRowInfo(t.TableId);
            }
        }

        // ─── Add ────────────────────────────────────────────────────────────
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admin users can add tables.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var addForm = new FormAddTable();
            addForm.StartPosition = FormStartPosition.CenterParent;
            if (addForm.ShowDialog(this) != DialogResult.OK) return;

            var newTable = new Table
            {
                TableName = addForm.TableName,
                Capacity = addForm.Capacity,
                Status = addForm.Status
            };

            // ✅ COMMAND PATTERN (AddTableCommand)
            var cmd = new AddTableCommand(newTable, _repo);
            try
            {
                _invoker.ExecuteCommand(cmd);
                LoadTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding table:\n" + ex.Message, "Error");
            }
        }

        // ─── Edit / Delete cell click ───────────────────────────────────────
        private void dgvTable_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dgvTable.Columns[e.ColumnIndex].Name;
            if (colName != "colEdit" && colName != "colDelete") return;

            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admin users can modify or delete tables.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvTable.Rows[e.RowIndex];
            var info = row.Tag is TableRowInfo t ? t : new TableRowInfo(-1);
            int tableId = info.TableId;

            // ✅ EDIT → COMMAND PATTERN (UpdateTableCommand)
            if (colName == "colEdit")
            {
                // ✅ REPOSITORY PATTERN: fresh data from DB
                var table = _repo.GetById(tableId);
                if (table == null) return;

                using var editForm = new FormAddTable(
                    table.TableId, table.TableName, table.Capacity, table.Status);
                editForm.StartPosition = FormStartPosition.CenterParent;
                if (editForm.ShowDialog(this) != DialogResult.OK) return;

                var updated = new Table
                {
                    TableId = table.TableId,
                    TableName = editForm.TableName,
                    Capacity = editForm.Capacity,
                    Status = editForm.Status
                };

                // ✅ COMMAND PATTERN
                var cmd = new UpdateTableCommand(updated, _repo);
                try
                {
                    _invoker.ExecuteCommand(cmd);
                    LoadTables();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating table:\n" + ex.Message, "Error");
                }
            }

            // ✅ DELETE → COMMAND PATTERN (DeleteTableCommand)
            else if (colName == "colDelete")
            {
                string tableName = row.Cells["colTable"].Value?.ToString() ?? "";
                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete '{tableName}'?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                // ✅ COMMAND PATTERN
                var cmd = new DeleteTableCommand(tableId, _repo);
                try
                {
                    _invoker.ExecuteCommand(cmd);
                    LoadTables();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting table:\n" + ex.Message, "Error");
                }
            }
        }

        // ─── Undo / Redo (Ctrl+Z / Ctrl+Y) ─────────────────────────────────
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z)) { PerformUndo(); return true; }
            if (keyData == (Keys.Control | Keys.Y)) { PerformRedo(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void PerformUndo()
        {
            if (!_invoker.CanUndo) { MessageBox.Show("Nothing to undo."); return; }
            try { _invoker.Undo(); LoadTables(); }
            catch (Exception ex) { MessageBox.Show("Undo failed: " + ex.Message); }
        }

        private void PerformRedo()
        {
            if (!_invoker.CanRedo) { MessageBox.Show("Nothing to redo."); return; }
            try { _invoker.Redo(); LoadTables(); }
            catch (Exception ex) { MessageBox.Show("Redo failed: " + ex.Message); }
        }
    }
}