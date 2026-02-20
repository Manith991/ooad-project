using System.Data;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Command;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project
{
    /// <summary>
    /// COMMAND PATTERN applied:
    ///   - Uses AddProductCommand / UpdateProductCommand via CommandInvoker
    ///     so every save is undoable from the parent form's invoker.
    /// REPOSITORY PATTERN applied:
    ///   - Category list loaded through CategoryRepository instead of raw SQL.
    /// </summary>
    public partial class FormAddProduct : Form
    {
        // ── Public properties read by the caller ──────────────────────────
        public string ProductName => txtProduct.Text.Trim();
        public decimal Price
        {
            get { decimal.TryParse(txtPrice.Text, out decimal v); return v; }
        }
        public int CategoryID
        {
            get
            {
                if (cbCategory.SelectedValue != null)
                    return Convert.ToInt32(cbCategory.SelectedValue);
                return -1;
            }
        }
        public string ImagePath => _imagePath;

        // ── Private state ─────────────────────────────────────────────────
        private bool _isEditMode = false;
        private int _productId = -1;
        private string _imagePath = string.Empty;

        // REPOSITORY PATTERN – injected or created locally
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;

        // COMMAND PATTERN – shared invoker from parent (or local)
        private readonly CommandInvoker _invoker;

        // ── Constructor: Add mode ─────────────────────────────────────────
        public FormAddProduct(
            CommandInvoker? invoker = null,
            IRepository<Product>? productRepository = null,
            IRepository<Category>? categoryRepository = null)
        {
            InitializeComponent();
            this.Text = "Add Product";
            btnSave.Text = "Add";
            lblProduct.Text = "Add Product";

            _invoker = invoker ?? new CommandInvoker();
            _productRepository = productRepository ?? new ProductRepository();
            _categoryRepository = categoryRepository ?? new CategoryRepository();

            LoadCategories();
        }

        // ── Constructor: Edit mode ────────────────────────────────────────
        public FormAddProduct(
            int id,
            string productName,
            decimal price,
            int categoryId,
            string imagePath = "",
            CommandInvoker? invoker = null,
            IRepository<Product>? productRepository = null,
            IRepository<Category>? categoryRepository = null)
        {
            InitializeComponent();
            this.Text = "Edit Product";
            btnSave.Text = "Save";
            lblProduct.Text = "Edit Product";

            _isEditMode = true;
            _productId = id;
            _imagePath = imagePath;

            _invoker = invoker ?? new CommandInvoker();
            _productRepository = productRepository ?? new ProductRepository();
            _categoryRepository = categoryRepository ?? new CategoryRepository();

            txtProduct.Text = productName;
            txtPrice.Text = price.ToString();

            LoadCategories();
            cbCategory.SelectedValue = categoryId;

            // Load image preview if path exists
            if (!string.IsNullOrEmpty(imagePath))
            {
                string fullPath = imagePath;

                if (!File.Exists(fullPath))
                {
                    string tryPath = Path.Combine(
                        Application.StartupPath, "Resources", "Foods", imagePath + ".png");
                    if (File.Exists(tryPath)) fullPath = tryPath;
                }

                if (File.Exists(fullPath))
                    pbIcon.Image = Image.FromFile(fullPath);
            }
        }

        // ── Load categories via Repository Pattern ────────────────────────
        private void LoadCategories()
        {
            try
            {
                var categories = _categoryRepository.GetAll().ToList();

                if (categories.Count == 0)
                {
                    MessageBox.Show("No categories found. Please add a category first.");
                    return;
                }

                // Bind as a DataTable so the ComboBox keeps DisplayMember / ValueMember
                var dt = new DataTable();
                dt.Columns.Add("categoryid", typeof(int));
                dt.Columns.Add("categoryname", typeof(string));

                foreach (var cat in categories)
                    dt.Rows.Add(cat.CategoryId, cat.CategoryName);

                cbCategory.DataSource = dt;
                cbCategory.DisplayMember = "categoryname";
                cbCategory.ValueMember = "categoryid";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories:\n" + ex.Message);
            }
        }

        // ── Browse image ──────────────────────────────────────────────────
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select Product Image",
                Filter = "Image Files|*.jpg;*.jpeg;*.png"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _imagePath = ofd.FileName;
                pbIcon.Image = Image.FromFile(_imagePath);
            }
        }

        // ── Save – COMMAND PATTERN ────────────────────────────────────────
        private void btnSave_Click(object sender, EventArgs e)
        {
            // ── Validation ────────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(txtProduct.Text))
            {
                MessageBox.Show("Enter a product name.");
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Invalid price format.");
                return;
            }
            if (cbCategory.SelectedValue == null)
            {
                MessageBox.Show("Please select a category.");
                return;
            }

            // ── Build domain object ───────────────────────────────────────
            var product = new Product
            {
                ProductId = _isEditMode ? _productId : 0,
                ProductName = txtProduct.Text.Trim(),
                Price = price,
                CategoryId = Convert.ToInt32(cbCategory.SelectedValue),
                ImagePath = _imagePath
            };

            try
            {
                ICommand cmd;

                if (_isEditMode)
                {
                    // COMMAND PATTERN: encapsulate update with undo support
                    cmd = new UpdateProductCommand(product, _productRepository);
                }
                else
                {
                    // COMMAND PATTERN: encapsulate add with undo support
                    cmd = new ProductCommand(product, _productRepository);
                }

                _invoker.ExecuteCommand(cmd);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving product:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}