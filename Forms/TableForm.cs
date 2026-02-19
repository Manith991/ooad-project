using System;
using System.Windows.Forms;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;
using OOAD_Project.Patterns.Command;
using OOAD_Project.Patterns.TemplateMethod;

namespace OOAD_Project
{
    public partial class TableForm : BaseCrudForm<Table>
    {
        private readonly record struct TableRowInfo(int TableId);
        private readonly IRepository<Table> _repo;

        public TableForm(string userRole) : base(userRole)
        {
            InitializeComponent();

            // wire base-class fields BEFORE InitializeForm()
            this.dataGridView = dgvTable;
            this.btnAdd = btnAdd;

            _repo = new TableRepository();
            InitializeForm();   // ← template method skeleton runs here
        }

        // ── Abstract steps ────────────────────────────────────────────────

        protected override void LoadData()
        {
            dgvTable.Rows.Clear();
            var tables = _repo.GetAll();
            int rowNo = 1;
            foreach (var t in tables)
            {
                int idx = dgvTable.Rows.Add(rowNo++, t.TableName, t.Capacity, t.Status);
                dgvTable.Rows[idx].Tag = new TableRowInfo(t.TableId);
            }
        }

        protected override Table GetEntityFromRow(DataGridViewRow row)
        {
            var info = row.Tag is TableRowInfo t ? t : new TableRowInfo(-1);
            return _repo.GetById(info.TableId)!;
        }

        protected override int GetEntityId(DataGridViewRow row) =>
            row.Tag is TableRowInfo t ? t.TableId : -1;

        protected override void OnEdit(Table table)
        {
            using var form = new FormAddTable(
                table.TableId, table.TableName, table.Capacity, table.Status);
            form.StartPosition = FormStartPosition.CenterParent;
            if (form.ShowDialog(this) != DialogResult.OK) return;

            var updated = new Table
            {
                TableId = table.TableId,
                TableName = form.TableName,
                Capacity = form.Capacity,
                Status = form.Status
            };

            try { commandInvoker.ExecuteCommand(new UpdateTableCommand(updated, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error updating table:\n" + ex.Message, "Error"); }
        }

        protected override void OnDelete(int id)
        {
            try { commandInvoker.ExecuteCommand(new DeleteTableCommand(id, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error deleting table:\n" + ex.Message, "Error"); }
        }

        // ── Override Add ──────────────────────────────────────────────────

        protected override void OnAddClick(object? sender, EventArgs e)
        {
            base.OnAddClick(sender, e); // access-denied guard for non-admins
            if (userRole != "(admin)") return;

            using var form = new FormAddTable();
            form.StartPosition = FormStartPosition.CenterParent;
            if (form.ShowDialog(this) != DialogResult.OK) return;

            var newTable = new Table
            {
                TableName = form.TableName,
                Capacity = form.Capacity,
                Status = form.Status
            };

            try { commandInvoker.ExecuteCommand(new AddTableCommand(newTable, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error adding table:\n" + ex.Message, "Error"); }
        }
    }
}