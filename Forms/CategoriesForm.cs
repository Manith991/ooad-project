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
    /// CategoriesForm – uses:
    ///   REPOSITORY PATTERN : CategoryRepository handles all DB access
    ///   COMMAND PATTERN    : Add / Edit / Delete are undoable ICommands
    /// </summary>
    public partial class CategoriesForm : Form
    {
        // ✅ REPOSITORY PATTERN
        private readonly IRepository<Category> _repo;

        // ✅ COMMAND PATTERN
        private readonly CommandInvoker _invoker = new CommandInvoker();

        private readonly string userRole;

        public CategoriesForm(string userRole)
        {
            InitializeComponent();
            this.userRole = userRole;

            // ✅ REPOSITORY PATTERN: single place for all category DB calls
            _repo = new CategoryRepository();

            LoadCategories();
            RestrictActionsByRole();
        }

        // ─── Role restriction ───────────────────────────────────────────────
        private void RestrictActionsByRole()
        {
            if (userRole != "(admin)")
            {
                if (dgvCategory.Columns.Contains("colEdit"))
                    dgvCategory.Columns["colEdit"].Visible = false;
                if (dgvCategory.Columns.Contains("colDelete"))
                    dgvCategory.Columns["colDelete"].Visible = false;
            }
        }

        // ─── Load ───────────────────────────────────────────────────────────
        private void LoadCategories()
        {
            dgvCategory.Rows.Clear();

            // ✅ REPOSITORY PATTERN
            var categories = _repo.GetAll();
            int rowNo = 1;

            foreach (var cat in categories)
            {
                Image? img = LoadImage(cat.ImagePath);
                int rowIndex = dgvCategory.Rows.Add(rowNo++, cat.CategoryName, img);
                dgvCategory.Rows[rowIndex].Tag = cat.CategoryId;
            }
        }

        // ─── Edit / Delete cell click ───────────────────────────────────────
        private void dgvCategory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admins can modify or delete categories.",
                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string colName = dgvCategory.Columns[e.ColumnIndex].Name;
            var row = dgvCategory.Rows[e.RowIndex];
            int categoryId = row.Tag is int id ? id : -1;
            string categoryName = row.Cells["colCategory"].Value?.ToString() ?? "";
            Image? existingImage = row.Cells["colIcon"].Value as Image;

            // ✅ EDIT → COMMAND PATTERN (UpdateCategoryCommand)
            if (colName == "colEdit")
            {
                using var editForm = new FormEditCate(categoryId, categoryName, existingImage);
                if (editForm.ShowDialog(this) != DialogResult.OK) return;

                // ✅ REPOSITORY PATTERN: repository fetches the old state internally
                var updated = new Category
                {
                    CategoryId = categoryId,
                    CategoryName = editForm.CategoryName,
                    ImagePath = string.IsNullOrEmpty(editForm.ImagePath)
                                       ? null
                                       : Path.GetFileNameWithoutExtension(editForm.ImagePath)
                };

                // ✅ COMMAND PATTERN
                var cmd = new UpdateCategoryCommand(updated, _repo);
                try
                {
                    _invoker.ExecuteCommand(cmd);
                    MessageBox.Show("Category updated successfully!");
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating category:\n" + ex.Message, "Error");
                }
            }

            // ✅ DELETE → COMMAND PATTERN (DeleteCategoryCommand)
            else if (colName == "colDelete")
            {
                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete '{categoryName}'?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                // ✅ COMMAND PATTERN
                var cmd = new DeleteCategoryCommand(categoryId, _repo);
                try
                {
                    _invoker.ExecuteCommand(cmd);
                    LoadCategories();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting category:\n" + ex.Message, "Error");
                }
            }
        }

        // ─── Add button ─────────────────────────────────────────────────────
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admins can add categories.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var addForm = new FormEditCate(0, "New Category");
            addForm.Text = "Add Category";
            if (addForm.ShowDialog(this) != DialogResult.OK) return;

            var newCat = new Category
            {
                CategoryName = addForm.CategoryName,
                ImagePath = string.IsNullOrEmpty(addForm.ImagePath)
                                   ? null
                                   : Path.GetFileNameWithoutExtension(addForm.ImagePath)
            };

            // ✅ COMMAND PATTERN (AddCategoryCommand)
            var cmd = new AddCategoryCommand(newCat, _repo);
            try
            {
                _invoker.ExecuteCommand(cmd);
                LoadCategories();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding category:\n" + ex.Message, "Error");
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
            try { _invoker.Undo(); LoadCategories(); }
            catch (Exception ex) { MessageBox.Show("Undo failed: " + ex.Message); }
        }

        private void PerformRedo()
        {
            if (!_invoker.CanRedo) { MessageBox.Show("Nothing to redo."); return; }
            try { _invoker.Redo(); LoadCategories(); }
            catch (Exception ex) { MessageBox.Show("Redo failed: " + ex.Message); }
        }

        // ─── Image helper ────────────────────────────────────────────────────
        private Image? LoadImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return GetDefaultImage();

            string[] paths =
            {
                imagePath,
                Path.Combine(Application.StartupPath, "Resources", imagePath),
                Path.Combine(Application.StartupPath, "Resources", imagePath + ".png"),
                Path.Combine(Application.StartupPath, "Resources", imagePath + ".jpg")
            };

            foreach (var p in paths)
                if (File.Exists(p)) return Image.FromFile(p);

            return GetDefaultImage();
        }

        private Image? GetDefaultImage()
        {
            string def = Path.Combine(Application.StartupPath, "Resources", "no_image.png");
            return File.Exists(def) ? Image.FromFile(def) : null;
        }
    }
}