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
    /// ProductForm – uses:
    ///   REPOSITORY PATTERN : ProductRepository handles all DB access
    ///   COMMAND PATTERN    : Add / Edit / Delete are undoable ICommands
    /// </summary>
    public partial class ProductForm : Form
    {
        // Row metadata stored in DataGridViewRow.Tag
        private readonly record struct ProductRowInfo(int ProductId, int CategoryId, string? ImagePath);

        // ✅ REPOSITORY PATTERN
        private readonly IRepository<Product> _repo;

        // ✅ COMMAND PATTERN
        private readonly CommandInvoker _invoker = new CommandInvoker();

        private readonly string userRole;

        public ProductForm(string userRole)
        {
            InitializeComponent();
            this.userRole = userRole;

            // ✅ REPOSITORY PATTERN
            _repo = new ProductRepository();

            LoadProducts();
            RestrictActionsByRole();
        }

        // ─── Role restriction ───────────────────────────────────────────────
        private void RestrictActionsByRole()
        {
            if (userRole != "(admin)")
            {
                btnAdd.Enabled = false;
                if (dgvProduct.Columns.Contains("colEdit")) dgvProduct.Columns["colEdit"].Visible = false;
                if (dgvProduct.Columns.Contains("colDelete")) dgvProduct.Columns["colDelete"].Visible = false;
            }
        }

        // ─── Load ───────────────────────────────────────────────────────────
        private void LoadProducts()
        {
            dgvProduct.Rows.Clear();

            // ✅ REPOSITORY PATTERN
            var products = _repo.GetAll();
            int rowNo = 1;

            foreach (var p in products)
            {
                Image? img = LoadImage(p.ImagePath, "Foods");
                int rowIndex = dgvProduct.Rows.Add(rowNo++, p.ProductName,
                                                   p.Price.ToString("0.00"), p.CategoryName, img);
                dgvProduct.Rows[rowIndex].Tag =
                    new ProductRowInfo(p.ProductId, p.CategoryId ?? -1, p.ImagePath);
            }
        }

        // ─── Add ────────────────────────────────────────────────────────────
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admins can add products.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var addForm = new FormAddProduct();
            addForm.StartPosition = FormStartPosition.CenterParent;
            if (addForm.ShowDialog(this) != DialogResult.OK) return;

            var newProduct = new Product
            {
                ProductName = addForm.ProductName,
                Price = addForm.Price,
                CategoryId = addForm.CategoryID,
                ImagePath = string.IsNullOrEmpty(addForm.ImagePath)
                                  ? null
                                  : Path.GetFileNameWithoutExtension(addForm.ImagePath)
            };

            // ✅ COMMAND PATTERN (ProductCommand = AddProductCommand)
            var cmd = new ProductCommand(newProduct, _repo);
            try
            {
                _invoker.ExecuteCommand(cmd);
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding product:\n" + ex.Message, "Error");
            }
        }

        // ─── Edit / Delete cell click ───────────────────────────────────────
        private void dgvProduct_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admins can modify or delete products.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string colName = dgvProduct.Columns[e.ColumnIndex].Name;
            var row = dgvProduct.Rows[e.RowIndex];
            var info = row.Tag is ProductRowInfo pri ? pri : new ProductRowInfo(-1, -1, null);

            // ✅ EDIT → COMMAND PATTERN (UpdateProductCommand)
            if (colName == "colEdit")
            {
                // ✅ REPOSITORY PATTERN: get full product data
                var product = _repo.GetById(info.ProductId);
                if (product == null) return;

                using var editForm = new FormAddProduct(
                    product.ProductId, product.ProductName,
                    product.Price, product.CategoryId ?? -1, product.ImagePath);
                editForm.StartPosition = FormStartPosition.CenterParent;
                if (editForm.ShowDialog(this) != DialogResult.OK) return;

                var updated = new Product
                {
                    ProductId = product.ProductId,
                    ProductName = editForm.ProductName,
                    Price = editForm.Price,
                    CategoryId = editForm.CategoryID,
                    ImagePath = string.IsNullOrEmpty(editForm.ImagePath)
                                      ? product.ImagePath
                                      : Path.GetFileNameWithoutExtension(editForm.ImagePath)
                };

                // ✅ COMMAND PATTERN
                var cmd = new UpdateProductCommand(updated, _repo);
                try
                {
                    _invoker.ExecuteCommand(cmd);
                    MessageBox.Show("Product updated successfully!");
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating product:\n" + ex.Message, "Error");
                }
            }

            // ✅ DELETE → COMMAND PATTERN (DeleteProductCommand)
            else if (colName == "colDelete")
            {
                string name = row.Cells["colProduct"].Value?.ToString() ?? "";
                var confirm = MessageBox.Show(
                    $"Are you sure you want to delete '{name}'?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                // ✅ COMMAND PATTERN
                var cmd = new DeleteProductCommand(info.ProductId, _repo);
                try
                {
                    _invoker.ExecuteCommand(cmd);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting product:\n" + ex.Message, "Error");
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
            try { _invoker.Undo(); LoadProducts(); }
            catch (Exception ex) { MessageBox.Show("Undo failed: " + ex.Message); }
        }

        private void PerformRedo()
        {
            if (!_invoker.CanRedo) { MessageBox.Show("Nothing to redo."); return; }
            try { _invoker.Redo(); LoadProducts(); }
            catch (Exception ex) { MessageBox.Show("Redo failed: " + ex.Message); }
        }

        // ─── Image helper ────────────────────────────────────────────────────
        private Image? LoadImage(string? imagePath, string subfolder = "")
        {
            if (string.IsNullOrEmpty(imagePath)) return GetDefaultImage();

            string resBase = Path.Combine(Application.StartupPath, "Resources");
            string sub = string.IsNullOrEmpty(subfolder)
                                 ? resBase
                                 : Path.Combine(resBase, subfolder);

            string[] paths =
            {
                imagePath,
                Path.Combine(sub, imagePath),
                Path.Combine(sub, imagePath + ".png"),
                Path.Combine(sub, imagePath + ".jpg"),
                Path.Combine(sub, imagePath + ".jpeg")
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