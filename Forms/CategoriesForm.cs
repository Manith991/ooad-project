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
    public partial class CategoriesForm : BaseCrudForm<Category>
    {
        private readonly IRepository<Category> _repo;

        public CategoriesForm(string userRole) : base(userRole)
        {
            InitializeComponent();
            this.dataGridView = dgvCategory;
            this.btnAdd = btnAdd;

            _repo = new CategoryRepository();
            InitializeForm();
        }

        // ── Abstract steps ────────────────────────────────────────────────

        protected override void LoadData()
        {
            dgvCategory.Rows.Clear();
            var cats = _repo.GetAll();
            int rowNo = 1;
            foreach (var cat in cats)
            {
                Image? img = LoadImage(cat.ImagePath);
                int idx = dgvCategory.Rows.Add(rowNo++, cat.CategoryName, img);
                dgvCategory.Rows[idx].Tag = cat.CategoryId;
            }
        }

        protected override Category GetEntityFromRow(DataGridViewRow row)
        {
            int id = row.Tag is int i ? i : -1;
            string name = row.Cells["colCategory"].Value?.ToString() ?? "";
            Image? img = row.Cells["colIcon"].Value as Image;
            return new Category { CategoryId = id, CategoryName = name };
        }

        protected override int GetEntityId(DataGridViewRow row) =>
            row.Tag is int i ? i : -1;

        protected override void OnEdit(Category cat)
        {
            Image? existingImg = null;
            // find the row to get the icon image
            foreach (DataGridViewRow r in dgvCategory.Rows)
                if (r.Tag is int id && id == cat.CategoryId)
                { existingImg = r.Cells["colIcon"].Value as Image; break; }

            using var form = new FormEditCate(cat.CategoryId, cat.CategoryName, existingImg);
            if (form.ShowDialog(this) != DialogResult.OK) return;

            var updated = new Category
            {
                CategoryId = cat.CategoryId,
                CategoryName = form.CategoryName,
                ImagePath = string.IsNullOrEmpty(form.ImagePath)
                                   ? null
                                   : Path.GetFileNameWithoutExtension(form.ImagePath)
            };

            try
            {
                commandInvoker.ExecuteCommand(new UpdateCategoryCommand(updated, _repo));
                MessageBox.Show("Category updated successfully!");
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Error updating category:\n" + ex.Message, "Error"); }
        }

        protected override void OnDelete(int id)
        {
            try { commandInvoker.ExecuteCommand(new DeleteCategoryCommand(id, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error deleting category:\n" + ex.Message, "Error"); }
        }

        protected override void OnAddClick(object? sender, EventArgs e)
        {
            base.OnAddClick(sender, e);
            if (userRole != "(admin)") return;

            using var form = new FormEditCate(0, "New Category");
            form.Text = "Add Category";
            if (form.ShowDialog(this) != DialogResult.OK) return;

            var newCat = new Category
            {
                CategoryName = form.CategoryName,
                ImagePath = string.IsNullOrEmpty(form.ImagePath)
                                   ? null
                                   : Path.GetFileNameWithoutExtension(form.ImagePath)
            };

            try { commandInvoker.ExecuteCommand(new AddCategoryCommand(newCat, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error adding category:\n" + ex.Message, "Error"); }
        }

        // ── CategoriesForm doesn't disable btnAdd for non-admins ──────────
        protected override void RestrictActionsByRole()
        {
            if (userRole == "(admin)") return;
            // only hide Edit/Delete columns; btnAdd stays enabled (shows access-denied on click)
            if (dgvCategory.Columns.Contains("colEdit")) dgvCategory.Columns["colEdit"].Visible = false;
            if (dgvCategory.Columns.Contains("colDelete")) dgvCategory.Columns["colDelete"].Visible = false;
        }
    }
}