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
    public partial class ProductForm : BaseCrudForm<Product>
    {
        private readonly record struct ProductRowInfo(int ProductId, int CategoryId, string? ImagePath);
        private readonly IRepository<Product> _repo;

        public ProductForm(string userRole) : base(userRole)
        {
            InitializeComponent();
            this.dataGridView = dgvProduct;
            this.btnAdd = btnAdd;

            _repo = new ProductRepository();
            InitializeForm();
        }

        // ── Abstract steps ────────────────────────────────────────────────

        protected override void LoadData()
        {
            dgvProduct.Rows.Clear();
            var products = _repo.GetAll();
            int rowNo = 1;
            foreach (var p in products)
            {
                Image? img = LoadImage(p.ImagePath, "Foods");
                int idx = dgvProduct.Rows.Add(rowNo++, p.ProductName, p.Price.ToString("0.00"), p.CategoryName, img);
                dgvProduct.Rows[idx].Tag = new ProductRowInfo(p.ProductId, p.CategoryId ?? -1, p.ImagePath);
            }
        }

        protected override Product GetEntityFromRow(DataGridViewRow row)
        {
            var info = row.Tag is ProductRowInfo pri ? pri : new ProductRowInfo(-1, -1, null);
            return _repo.GetById(info.ProductId)!;
        }

        protected override int GetEntityId(DataGridViewRow row) =>
            row.Tag is ProductRowInfo pri ? pri.ProductId : -1;

        protected override void OnEdit(Product product)
        {
            using var form = new FormAddProduct(
                product.ProductId, product.ProductName,
                product.Price, product.CategoryId ?? -1, product.ImagePath);
            form.StartPosition = FormStartPosition.CenterParent;
            if (form.ShowDialog(this) != DialogResult.OK) return;

            var updated = new Product
            {
                ProductId = product.ProductId,
                ProductName = form.ProductName,
                Price = form.Price,
                CategoryId = form.CategoryID,
                ImagePath = string.IsNullOrEmpty(form.ImagePath)
                                  ? product.ImagePath
                                  : Path.GetFileNameWithoutExtension(form.ImagePath)
            };

            try
            {
                commandInvoker.ExecuteCommand(new UpdateProductCommand(updated, _repo));
                MessageBox.Show("Product updated successfully!");
                LoadData();
            }
            catch (Exception ex) { MessageBox.Show("Error updating product:\n" + ex.Message, "Error"); }
        }

        protected override void OnDelete(int id)
        {
            try { commandInvoker.ExecuteCommand(new DeleteProductCommand(id, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error deleting product:\n" + ex.Message, "Error"); }
        }

        protected override void OnAddClick(object? sender, EventArgs e)
        {
            base.OnAddClick(sender, e);
            if (userRole != "(admin)") return;

            using var form = new FormAddProduct();
            form.StartPosition = FormStartPosition.CenterParent;
            if (form.ShowDialog(this) != DialogResult.OK) return;

            var newProduct = new Product
            {
                ProductName = form.ProductName,
                Price = form.Price,
                CategoryId = form.CategoryID,
                ImagePath = string.IsNullOrEmpty(form.ImagePath)
                                  ? null
                                  : Path.GetFileNameWithoutExtension(form.ImagePath)
            };

            try { commandInvoker.ExecuteCommand(new ProductCommand(newProduct, _repo)); LoadData(); }
            catch (Exception ex) { MessageBox.Show("Error adding product:\n" + ex.Message, "Error"); }
        }
    }
}