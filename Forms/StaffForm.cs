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

        public StaffForm(string userRole) : base(userRole)
        {
            InitializeComponent();
            this.dataGridView = dgvStaff;
            this.btnAdd = btnAdd;

            _repo = new UserRepository();
            InitializeForm();
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

            try { commandInvoker.ExecuteCommand(new UpdateStaffCommand(updated, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error updating staff:\n" + ex.Message, "Error"); }
        }

        protected override void OnDelete(int id)
        {
            try { commandInvoker.ExecuteCommand(new DeleteStaffCommand(id, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error deleting staff:\n" + ex.Message, "Error"); }
        }

        // ── Image double-click (StaffForm-specific extra behavior) ────────

        protected override void SetupEventHandlers()
        {
            base.SetupEventHandlers();
            dgvStaff.CellDoubleClick -= DgvStaff_CellDoubleClick;
            dgvStaff.CellDoubleClick += DgvStaff_CellDoubleClick;
        }

        private void DgvStaff_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || dgvStaff.Columns[e.ColumnIndex].Name != "colImage") return;
            if (userRole != "(admin)")
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

            try { commandInvoker.ExecuteCommand(new UpdateStaffCommand(user, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error updating image:\n" + ex.Message, "Error"); }
        }
    }
}