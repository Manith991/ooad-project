using OOAD_Project.Patterns;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.Design.AxImporter;

namespace OOAD_Project
{
    /// <summary>
    /// Category-Aware Product Customization Dialog
    /// Shows different customization options based on product category
    /// IMPROVED PREVIEW SECTION - Better space utilization
    /// </summary>
    public partial class FormProductCustomization : Form
    {
        private IProduct _product;
        private decimal _basePrice;
        private string _category;

        // UI Controls
        private Label lblProductName;
        private Label lblCategory;
        private Label lblBasePrice;
        private Panel mainPanel;

        // Dynamic customization groups
        private GroupBox gbExtras;
        private GroupBox gbSize;
        private GroupBox gbTemperature;
        private GroupBox gbSweetness;
        private GroupBox gbDiscounts;
        private CheckBox chkAddTax;
        private Label lblFinalPrice;
        private Label lblFinalDescription;
        private Button btnAdd;
        private Button btnCancel;

        // Customization controls
        private Dictionary<string, CheckBox> extraCheckboxes = new Dictionary<string, CheckBox>();
        private Dictionary<string, RadioButton> sizeRadioButtons = new Dictionary<string, RadioButton>();
        private Dictionary<string, RadioButton> temperatureRadioButtons = new Dictionary<string, RadioButton>();
        private Dictionary<string, RadioButton> sweetnessRadioButtons = new Dictionary<string, RadioButton>();
        private Dictionary<string, CheckBox> discountCheckboxes = new Dictionary<string, CheckBox>();

        public IProduct CustomizedProduct => _product;

        public FormProductCustomization(int productId, string productName, decimal basePrice, string category)
        {
            _basePrice = basePrice;
            _category = category;
            _product = new BaseProduct(productId, productName, basePrice, category);

            InitializeCustomComponents();
            LoadCategorySpecificOptions();
            UpdatePreview();
        }

        private void InitializeCustomComponents()
        {
            // Form settings
            this.Text = "Customize Your Order";
            this.Width = 550;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AutoScroll = true;

            int yPos = 20;

            // Product Name
            lblProductName = new Label
            {
                Text = _product.GetName(),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(lblProductName);
            yPos += 35;

            // Category
            lblCategory = new Label
            {
                Text = $"Category: {_category}",
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                ForeColor = Color.DarkGreen,
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(lblCategory);
            yPos += 30;

            // Base Price
            lblBasePrice = new Label
            {
                Text = $"Base Price: ${_basePrice:F2}",
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(20, yPos)
            };
            this.Controls.Add(lblBasePrice);
            yPos += 40;

            // Main panel for scrollable content
            mainPanel = new Panel
            {
                Location = new Point(20, yPos),
                Width = 490,
                Height = 430,
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(mainPanel);
            yPos += 440;

            // ========================================
            // ✅ IMPROVED PREVIEW SECTION
            // ========================================

            // Preview Panel with border for better visual separation
            var previewPanel = new Panel
            {
                Location = new Point(20, yPos),
                Width = 490,
                Height = 110,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(250, 250, 250)
            };
            this.Controls.Add(previewPanel);

            // "Order Summary" header
            var lblSummaryHeader = new Label
            {
                Text = "Order Summary",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = true,
                Location = new Point(10, 8)
            };
            previewPanel.Controls.Add(lblSummaryHeader);

            // Description (product details)
            lblFinalDescription = new Label
            {
                Text = "",
                Location = new Point(10, 35),
                Width = 465,
                Height = 40,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Red,
                AutoSize = false
            };
            previewPanel.Controls.Add(lblFinalDescription);

            // Total Price
            lblFinalPrice = new Label
            {
                Text = $"Total: ${_basePrice:F2}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Green,
                AutoSize = true,
                Location = new Point(5, 65)
            };
            previewPanel.Controls.Add(lblFinalPrice);

            yPos += 120;

            // ========================================
            // ✅ BUTTONS - Side by side with better spacing
            // ========================================
            btnAdd = new Button
            {
                Text = "Add to Order",
                Width = 240,
                Height = 50,
                Location = new Point(20, yPos),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                DialogResult = DialogResult.OK,
                Cursor = Cursors.Hand
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnAdd);

            btnCancel = new Button
            {
                Text = "Cancel",
                Width = 240,
                Height = 50,
                Location = new Point(270, yPos),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnCancel);
        }

        /// <summary>
        /// ✅ CATEGORY-AWARE CUSTOMIZATION
        /// Different options for different food categories
        /// </summary>
        private void LoadCategorySpecificOptions()
        {
            int yPos = 10;

            switch (_category.ToLower())
            {
                case "main dishes":
                case "main dish":
                case "seafood":
                    yPos = AddMainDishOptions(yPos);
                    break;

                case "appetizers":
                case "appetizer":
                    yPos = AddAppetizerOptions(yPos);
                    break;

                case "side dishes":
                case "side dish":
                    yPos = AddSideDishOptions(yPos);
                    break;

                case "soups/salads":
                case "soup":
                case "salad":
                    yPos = AddSoupSaladOptions(yPos);
                    break;

                case "beverages":
                case "beverage":
                    yPos = AddBeverageOptions(yPos);
                    break;

                case "desserts":
                case "dessert":
                    yPos = AddDessertOptions(yPos);
                    break;

                default:
                    yPos = AddGenericOptions(yPos);
                    break;
            }

            // Tax option (common to all)
            yPos += 10;
            chkAddTax = new CheckBox
            {
                Text = "Add Tax (10%)",
                Location = new Point(15, yPos),
                AutoSize = true,
                Checked = true
            };
            chkAddTax.CheckedChanged += OnCustomizationChanged;
            mainPanel.Controls.Add(chkAddTax);
            yPos += 35;

            // Discounts (common to all)
            yPos = AddDiscountOptions(yPos);
        }

        // ========================================
        // MAIN DISHES: Full customization
        // ========================================
        private int AddMainDishOptions(int yPos)
        {
            // Protein options
            yPos = AddGroupBox("Protein Add-ons", yPos, new[]
            {
                ("Extra Chicken", 3.00m),
                ("Extra Beef", 3.50m),
                ("Extra Cheese", 1.50m),
                ("Bacon", 2.00m),
                ("Avocado", 1.75m)
            }, extraCheckboxes);

            // Size options
            yPos = AddSizeGroup(yPos, true);

            // Cooking preference
            yPos = AddGroupBox("Cooking Preference", yPos, new[]
            {
                ("Well Done", 0m),
                ("Medium", 0m),
                ("Rare", 0m)
            }, sizeRadioButtons, isRadio: true);

            return yPos;
        }

        // ========================================
        // APPETIZERS: Light customization
        // ========================================
        private int AddAppetizerOptions(int yPos)
        {
            // Dipping sauces
            yPos = AddGroupBox("Sauces", yPos, new[]
            {
                ("Extra Sauce", 0.50m),
                ("Spicy Mayo", 0.75m),
                ("Garlic Aioli", 0.75m)
            }, extraCheckboxes);

            // Portion size
            yPos = AddGroupBox("Portion", yPos, new[]
            {
                ("Regular", 0m),
                ("Large (+$2)", 2.00m)
            }, sizeRadioButtons, isRadio: true);

            return yPos;
        }

        // ========================================
        // SIDE DISHES: Size only
        // ========================================
        private int AddSideDishOptions(int yPos)
        {
            // Size options
            yPos = AddSizeGroup(yPos, false);

            // Seasoning
            yPos = AddGroupBox("Seasoning", yPos, new[]
            {
                ("Extra Salt", 0m),
                ("Cajun Spice", 0.50m),
                ("Garlic Butter", 0.75m)
            }, extraCheckboxes);

            return yPos;
        }

        // ========================================
        // SOUPS/SALADS: Temperature & Dressing
        // ========================================
        private int AddSoupSaladOptions(int yPos)
        {
            // Temperature for soup
            if (_product.GetName().ToLower().Contains("soup"))
            {
                yPos = AddGroupBox("Temperature", yPos, new[]
                {
                    ("Hot", 0m),
                    ("Warm", 0m)
                }, temperatureRadioButtons, isRadio: true);
            }

            // Dressing for salad
            if (_product.GetName().ToLower().Contains("salad"))
            {
                yPos = AddGroupBox("Dressing", yPos, new[]
                {
                    ("Ranch", 0m),
                    ("Italian", 0m),
                    ("Caesar", 0m),
                    ("Vinaigrette", 0m)
                }, sizeRadioButtons, isRadio: true);

                yPos = AddGroupBox("Add-ons", yPos, new[]
                {
                    ("Grilled Chicken", 3.00m),
                    ("Avocado", 1.75m),
                    ("Extra Cheese", 1.50m)
                }, extraCheckboxes);
            }

            // Bowl size
            yPos = AddSizeGroup(yPos, false);

            return yPos;
        }

        // ========================================
        // BEVERAGES: Size, Temperature, Sweetness
        // ========================================
        private int AddBeverageOptions(int yPos)
        {
            // Size
            yPos = AddGroupBox("Size", yPos, new[]
            {
                ("Small", 0m),
                ("Medium (+$0.50)", 0.50m),
                ("Large (+$1.00)", 1.00m),
                ("Extra Large (+$1.50)", 1.50m)
            }, sizeRadioButtons, isRadio: true);

            // Temperature (for coffee/tea)
            if (_product.GetName().ToLower().Contains("coffee") ||
                _product.GetName().ToLower().Contains("tea"))
            {
                yPos = AddGroupBox("Temperature", yPos, new[]
                {
                    ("Hot", 0m),
                    ("Iced", 0m)
                }, temperatureRadioButtons, isRadio: true);
            }

            // Sweetness
            yPos = AddGroupBox("Sweetness", yPos, new[]
            {
                ("No Sugar", 0m),
                ("25% Sweet", 0m),
                ("50% Sweet", 0m),
                ("75% Sweet", 0m),
                ("100% Sweet", 0m)
            }, sweetnessRadioButtons, isRadio: true);

            // Add-ons
            yPos = AddGroupBox("Add-ons", yPos, new[]
            {
                ("Extra Shot (+$0.75)", 0.75m),
                ("Whipped Cream (+$0.50)", 0.50m),
                ("Caramel Drizzle (+$0.50)", 0.50m)
            }, extraCheckboxes);

            return yPos;
        }

        // ========================================
        // DESSERTS: Toppings & Size
        // ========================================
        private int AddDessertOptions(int yPos)
        {
            // Toppings
            yPos = AddGroupBox("Toppings", yPos, new[]
            {
                ("Whipped Cream", 0.75m),
                ("Chocolate Sauce", 0.75m),
                ("Caramel Sauce", 0.75m),
                ("Sprinkles", 0.50m),
                ("Ice Cream Scoop", 2.00m),
                ("Fresh Berries", 1.50m)
            }, extraCheckboxes);

            // Size
            yPos = AddGroupBox("Size", yPos, new[]
            {
                ("Regular", 0m),
                ("Large (+$2)", 2.00m)
            }, sizeRadioButtons, isRadio: true);

            return yPos;
        }

        // ========================================
        // GENERIC OPTIONS (fallback)
        // ========================================
        private int AddGenericOptions(int yPos)
        {
            yPos = AddSizeGroup(yPos, false);
            return yPos;
        }

        // ========================================
        // HELPER METHODS
        // ========================================
        private int AddSizeGroup(int yPos, bool includeExtraOptions)
        {
            var sizes = includeExtraOptions
                ? new[] { ("Regular", 0m), ("Large (+$2)", 2.00m), ("Extra Large (+$3.50)", 3.50m) }
                : new[] { ("Regular", 0m), ("Large (+$2)", 2.00m) };

            return AddGroupBox("Size", yPos, sizes, sizeRadioButtons, isRadio: true);
        }

        private int AddGroupBox(string title, int yPos, (string label, decimal price)[] options,
            Dictionary<string, CheckBox> checkboxDict)
        {
            var gb = new GroupBox
            {
                Text = title,
                Location = new Point(15, yPos),
                Width = 450,
                Height = 30 + (options.Length * 30)
            };

            int optionY = 25;

            foreach (var (label, price) in options)
            {
                var cb = new CheckBox
                {
                    Text = label,
                    Location = new Point(15, optionY),
                    AutoSize = true,
                    Tag = price
                };
                cb.CheckedChanged += OnCustomizationChanged;
                gb.Controls.Add(cb);
                checkboxDict[label] = cb;
                optionY += 30;
            }

            mainPanel.Controls.Add(gb);
            return yPos + gb.Height + 10;
        }

        private int AddGroupBox(string title, int yPos, (string label, decimal price)[] options,
            Dictionary<string, RadioButton> radioDict, bool isRadio = true)
        {
            var gb = new GroupBox
            {
                Text = title,
                Location = new Point(15, yPos),
                Width = 450,
                Height = 30 + (options.Length * 30)
            };

            int optionY = 25;
            bool isFirst = true;

            foreach (var (label, price) in options)
            {
                var rb = new RadioButton
                {
                    Text = label,
                    Location = new Point(15, optionY),
                    AutoSize = true,
                    Checked = isFirst,
                    Tag = price
                };
                rb.CheckedChanged += OnCustomizationChanged;
                gb.Controls.Add(rb);
                radioDict[label] = rb;
                isFirst = false;
                optionY += 30;
            }

            mainPanel.Controls.Add(gb);
            return yPos + gb.Height + 10;
        }

        private int AddDiscountOptions(int yPos)
        {
            // Define available discounts first so we can use discounts.Length when sizing the GroupBox
            var discounts = new[]
            {
                ("Student Discount (-15%)", 15m),
                ("Senior Citizen (-20%)", 20m),
                ("Happy Hour (-10%)", 10m)
            };

            var gb = new GroupBox
            {
                Text = "Discounts",
                Location = new Point(15, yPos),
                Width = 450,
                Height = 30 + (discounts.Length * 30)
            };

            int optionY = 25;
            foreach (var (label, percent) in discounts)
            {
                var cb = new CheckBox
                {
                    Text = label,
                    Location = new Point(15, optionY),
                    Tag = percent,
                    AutoSize = true
                };
                cb.CheckedChanged += (s, e) =>
                {
                    // Only one discount at a time
                    if (cb.Checked)
                    {
                        foreach (var other in discountCheckboxes.Values)
                        {
                            if (other != cb) other.Checked = false;
                        }
                    }
                    OnCustomizationChanged(s, e);
                };
                gb.Controls.Add(cb);
                discountCheckboxes[label] = cb;
                optionY += 30;
            }

            mainPanel.Controls.Add(gb);
            return yPos + gb.Height + 10;
        }

        private void OnCustomizationChanged(object? sender, EventArgs e)
        {
            UpdatePreview();
        }

        /// <summary>
        /// ✅ DECORATOR PATTERN IN ACTION
        /// Applies decorators based on user selections
        /// </summary>
        private void UpdatePreview()
        {
            // Start fresh
            _product = new BaseProduct(
                _product.GetProductId(),
                lblProductName.Text,
                _basePrice,
                _category
            );

            // Apply all checked extras
            foreach (var kvp in extraCheckboxes)
            {
                if (kvp.Value.Checked && kvp.Value.Tag is decimal price)
                {
                    string name = kvp.Key.Split('(')[0].Trim();
                    _product = new ExtraToppingDecorator(_product, name, price);
                }
            }

            // Apply selected size
            foreach (var kvp in sizeRadioButtons)
            {
                if (kvp.Value.Checked && kvp.Value.Tag is decimal price && price > 0)
                {
                    if (kvp.Key.Contains("Extra Large"))
                        _product = new ExtraLargeSizeDecorator(_product);
                    else if (kvp.Key.Contains("Large"))
                        _product = new LargeSizeDecorator(_product);
                }
            }

            // Apply discount (only one)
            foreach (var kvp in discountCheckboxes)
            {
                if (kvp.Value.Checked && kvp.Value.Tag is decimal percent)
                {
                    string reason = kvp.Key.Split('(')[0].Trim();
                    _product = new DiscountDecorator(_product, percent, reason);
                    break;
                }
            }

            // Apply tax
            if (chkAddTax != null && chkAddTax.Checked)
            {
                _product = new TaxDecorator(_product);
            }

            // Update UI
            lblFinalDescription.Text = _product.GetDescription();
            lblFinalPrice.Text = $"Total: ${_product.GetPrice():F2}";
        }
    }
}