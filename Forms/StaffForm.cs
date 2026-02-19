using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;
using OOAD_Project.Patterns.Command;

namespace OOAD_Project
{
    /// <summary>
    /// StaffForm – uses:
    ///   REPOSITORY PATTERN : UserRepository handles all DB access
    ///   COMMAND PATTERN    : Edit / Delete are undoable ICommands
    /// </summary>
    public partial class StaffForm : Form
    {
        // ✅ REPOSITORY PATTERN
        private readonly IRepository<User> _repo;

        // ✅ COMMAND PATTERN
        private readonly CommandInvoker _invoker = new CommandInvoker();

        private readonly string userRole;

        public StaffForm(string userRole)
        {
            InitializeComponent();
            this.userRole = userRole;

            // ✅ REPOSITORY PATTERN
            _repo = new UserRepository();

            LoadStaffData();
            RestrictActionsByRole();
        }

        // ─── Role restriction ───────────────────────────────────────────────
        private void RestrictActionsByRole()
        {
            if (userRole != "(admin)")
            {
                btnAdd.Enabled = false;
                if (dgvStaff.Columns.Contains("colEdit")) dgvStaff.Columns["colEdit"].Visible = false;
                if (dgvStaff.Columns.Contains("colDelete")) dgvStaff.Columns["colDelete"].Visible = false;
            }
        }

        // ─── Load ───────────────────────────────────────────────────────────
        private void LoadStaffData()
        {
            try
            {
                dgvStaff.Rows.Clear();

                // ✅ REPOSITORY PATTERN
                var users = _repo.GetAll();
                int rowNo = 1;

                foreach (var user in users)
                {
                    Image? img = LoadStaffImage(user.ImagePath);
                    int rowIndex = dgvStaff.Rows.Add(rowNo++, user.Name, user.Role, user.Status, img, null, null);
                    dgvStaff.Rows[rowIndex].Tag = Tuple.Create(user.Id, user.ImagePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading staff: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Cell click: Edit / Delete ──────────────────────────────────────
        private void dgvStaff_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admins can perform this action.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string colName = dgvStaff.Columns[e.ColumnIndex].Name;
            var row = dgvStaff.Rows[e.RowIndex];
            var tag = row.Tag as Tuple<int, string?>;
            if (tag == null) return;

            int userId = tag.Item1;
            string staffName = row.Cells["colStaff"].Value?.ToString() ?? "";
            string role = row.Cells["colRole"].Value?.ToString() ?? "";
            string status = row.Cells["colStatus"].Value?.ToString() ?? "";
            string? imgPath = tag.Item2;

            // ✅ EDIT → COMMAND PATTERN (UpdateStaffCommand)
            if (colName == "colEdit")
            {
                // ✅ REPOSITORY PATTERN: get fresh data
                var user = _repo.GetById(userId);
                if (user == null) return;

                using var editForm = new FormEditStaff(
                    user.Id, user.Name, user.Role, user.Status, user.ImagePath);
                editForm.StartPosition = FormStartPosition.CenterParent;

                if (editForm.ShowDialog() != DialogResult.OK) return;

                var updatedUser = new User
                {
                    Id = user.Id,
                    Username = user.Username,
                    Name = editForm.StaffNameValue,
                    Role = editForm.RoleValue,
                    Status = editForm.StatusValue,
                    ImagePath = editForm.CurrentImagePath
                };

                // ✅ COMMAND PATTERN
                var cmd = new UpdateStaffCommand(updatedUser, _repo);
                try
                {
                    _invoker.ExecuteCommand(cmd);
                    LoadStaffData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating staff: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // ✅ DELETE → COMMAND PATTERN (DeleteStaffCommand)
            else if (colName == "colDelete")
            {
                var confirm = MessageBox.Show(
                    $"Delete staff '{staffName}'?", "Confirm Delete",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                // ✅ COMMAND PATTERN
                var cmd = new DeleteStaffCommand(userId, _repo);
                try
                {
                    _invoker.ExecuteCommand(cmd);
                    LoadStaffData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting staff: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ─── Double-click image cell to change photo ─────────────────────────
        private void dgvStaff_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvStaff.Columns[e.ColumnIndex].Name != "colImage") return;

            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admins can change staff images.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvStaff.Rows[e.RowIndex];
            var tag = row.Tag as Tuple<int, string?>;
            if (tag == null) return;

            int userId = tag.Item1;

            using var ofd = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string destDir = Path.Combine(Application.StartupPath, "Resources");
            string destPath = Path.Combine(destDir, Path.GetFileName(ofd.FileName));
            Directory.CreateDirectory(destDir);
            File.Copy(ofd.FileName, destPath, true);

            // ✅ REPOSITORY PATTERN: build a User with the new image path and update via repo
            var user = _repo.GetById(userId);
            if (user == null) return;

            user.ImagePath = Path.GetFileName(ofd.FileName);

            // ✅ COMMAND PATTERN
            var cmd = new UpdateStaffCommand(user, _repo);
            try
            {
                _invoker.ExecuteCommand(cmd);
                LoadStaffData();
                MessageBox.Show("Image updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating image: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            try { _invoker.Undo(); LoadStaffData(); }
            catch (Exception ex) { MessageBox.Show("Undo failed: " + ex.Message); }
        }

        private void PerformRedo()
        {
            if (!_invoker.CanRedo) { MessageBox.Show("Nothing to redo."); return; }
            try { _invoker.Redo(); LoadStaffData(); }
            catch (Exception ex) { MessageBox.Show("Redo failed: " + ex.Message); }
        }

        // ─── Image helper ────────────────────────────────────────────────────
        private Image? LoadStaffImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return null;

            string[] possiblePaths =
            {
                imagePath,
                Path.Combine(Application.StartupPath, "Resources", imagePath),
                Path.Combine(Application.StartupPath, "Resources", imagePath + ".png"),
                Path.Combine(Application.StartupPath, "Resources", imagePath + ".jpg"),
                Path.Combine(Application.StartupPath, "Resources", imagePath + ".jpeg")
            };

            foreach (var p in possiblePaths)
            {
                try { if (File.Exists(p)) return Image.FromFile(p); }
                catch { /* skip locked files */ }
            }

            return null;
        }
    }
}