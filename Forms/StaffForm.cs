using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;
using OOAD_Project.Patterns.Command;
using OOAD_Project.Patterns.TemplateMethod;

namespace OOAD_Project
{
    public partial class StaffForm : BaseCrudForm<User>
    {
        private readonly IRepository<User> _repo;

        // ── Undo / Redo toolbar buttons (created in code so no designer change needed)
        private ToolStrip _toolStrip = new ToolStrip();
        private ToolStripButton _btnUndo = new ToolStripButton("↩ Undo") { Enabled = false };
        private ToolStripButton _btnRedo = new ToolStripButton("↪ Redo") { Enabled = false };

        public StaffForm(string userRole) : base(userRole)
        {
            InitializeComponent();
            this.dataGridView = dgvStaff;
            this.btnAdd = btnAdd;

            _repo = new UserRepository();

            BuildUndoRedoToolbar();
            InitializeForm();
        }

        // ── Build toolbar ─────────────────────────────────────────────────
        private void BuildUndoRedoToolbar()
        {
            _btnUndo.Click += (_, __) => UndoLast();
            _btnRedo.Click += (_, __) => RedoLast();

            _toolStrip.Items.Add(_btnUndo);
            _toolStrip.Items.Add(_btnRedo);

            // Insert toolbar at the top of the form
            this.Controls.Add(_toolStrip);
            _toolStrip.BringToFront();
        }

        private void RefreshUndoRedoButtons()
        {
            _btnUndo.Enabled = commandInvoker.CanUndo;
            _btnRedo.Enabled = commandInvoker.CanRedo;
            _btnUndo.Text = commandInvoker.CanUndo
                ? $"↩ Undo ({commandInvoker.GetUndoDescription()})" : "↩ Undo";
            _btnRedo.Text = commandInvoker.CanRedo
                ? $"↪ Redo ({commandInvoker.GetRedoDescription()})" : "↪ Redo";
        }

        private void UndoLast()
        {
            try { commandInvoker.Undo(); LoadData(); RefreshUndoRedoButtons(); }
            catch (Exception ex) { MessageBox.Show("Undo failed:\n" + ex.Message, "Error"); }
        }

        private void RedoLast()
        {
            try { commandInvoker.Redo(); LoadData(); RefreshUndoRedoButtons(); }
            catch (Exception ex) { MessageBox.Show("Redo failed:\n" + ex.Message, "Error"); }
        }

        // Helper: execute a command then refresh UI
        private void RunCommand(ICommand cmd)
        {
            commandInvoker.ExecuteCommand(cmd);
            LoadData();
            RefreshUndoRedoButtons();
        }

        // ── Abstract steps ────────────────────────────────────────────────

        protected override void LoadData()
        {
            dgvStaff.Rows.Clear();
            var users = _repo.GetAll();
            int rowNo = 1;
            foreach (var user in users)
            {
                Image? img = LoadImage(user.ImagePath);
                int idx = dgvStaff.Rows.Add(rowNo++, user.Name, user.Role, user.Status, img, null, null);
                dgvStaff.Rows[idx].Tag = Tuple.Create(user.Id, user.ImagePath);
            }
        }

        protected override User GetEntityFromRow(DataGridViewRow row)
        {
            var tag = row.Tag as Tuple<int, string?>;
            return tag != null ? _repo.GetById(tag.Item1)! : null!;
        }

        protected override int GetEntityId(DataGridViewRow row) =>
            row.Tag is Tuple<int, string?> t ? t.Item1 : -1;

        protected override void OnEdit(User user)
        {
            using var form = new FormEditStaff(
                user.Id, user.Name, user.Role, user.Status, user.ImagePath);
            form.StartPosition = FormStartPosition.CenterParent;
            if (form.ShowDialog() != DialogResult.OK) return;

            var updated = new User
            {
                Id = user.Id,
                Username = user.Username,
                Name = form.StaffNameValue,
                Role = form.RoleValue,
                Status = form.StatusValue,
                ImagePath = form.CurrentImagePath
            };

            try { RunCommand(new UpdateStaffCommand(updated, _repo)); }
            catch (Exception ex) { MessageBox.Show("Error updating staff:\n" + ex.Message, "Error"); }
        }

        protected override void OnDelete(int id)
        {
            try { RunCommand(new DeleteStaffCommand(id, _repo)); }
            catch (Exception ex) { MessageBox.Show("Error deleting staff:\n" + ex.Message, "Error"); }
        }

        protected override void SetupEventHandlers()
        {
            base.SetupEventHandlers();
            dgvStaff.CellDoubleClick -= DgvStaff_CellDoubleClick;
            dgvStaff.CellDoubleClick += DgvStaff_CellDoubleClick;
        }

        private void DgvStaff_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvStaff.Columns[e.ColumnIndex].Name != "colImage") return;
            if (userRole != "admin")
            {
                MessageBox.Show("Only admins can change staff images.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tag = dgvStaff.Rows[e.RowIndex].Tag as Tuple<int, string?>;
            if (tag == null) return;

            using var ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string dest = Path.Combine(Application.StartupPath, "Resources", Path.GetFileName(ofd.FileName));
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            File.Copy(ofd.FileName, dest, true);

            var user = _repo.GetById(tag.Item1);
            if (user == null) return;
            user.ImagePath = Path.GetFileName(ofd.FileName);

            try { RunCommand(new UpdateStaffCommand(user, _repo)); }
            catch (Exception ex) { MessageBox.Show("Error updating image:\n" + ex.Message, "Error"); }
        }
    }
}