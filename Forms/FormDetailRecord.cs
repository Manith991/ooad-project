using Npgsql;

namespace OOAD_Project
{
    public partial class FormDetailRecord : Form
    {
        private readonly int orderId;

        public FormDetailRecord(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;

            this.StartPosition = FormStartPosition.CenterScreen;

            LoadOrderDetail();
            LoadOrderProducts(); // ✅ Load all product items
        }

        private void LoadOrderDetail()
        {
            string query = @"
                SELECT 
                    o.order_id,
                    t.table_name,
                    u.name AS staff_name,
                    o.order_type,
                    o.order_date,
                    o.total_amount,
                    o.status
                FROM orders o
                LEFT JOIN tables t ON o.table_id = t.table_id
                LEFT JOIN users u ON o.user_id = u.id
                WHERE o.order_id = @order_id;
            ";

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@order_id", orderId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblID.Text = reader["order_id"].ToString();
                                lblTable.Text = reader["table_name"]?.ToString() ?? "-";
                                lblStaff.Text = reader["staff_name"]?.ToString() ?? "-";
                                lblType.Text = reader["order_type"]?.ToString() ?? "-";
                                lblDate.Text = Convert.ToDateTime(reader["order_date"]).ToString("yyyy-MM-dd HH:mm");
                                lblTotal.Text = Convert.ToDecimal(reader["total_amount"]).ToString("C");
                                lblStatus.Text = reader["status"]?.ToString() ?? "-";

                                string status = lblStatus.Text.ToLower();
                                lblStatus.ForeColor = status switch
                                {
                                    "completed" => Color.Green,
                                    "pending" => Color.Orange,
                                    "cancelled" => Color.Red,
                                    _ => Color.Black
                                };
                            }
                            else
                            {
                                MessageBox.Show("Order not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading order detail: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ✅ New method to load products
        private void LoadOrderProducts()
        {
            string query = @"
        SELECT 
            p.productname,
            od.quantity,
            od.price,
            od.subtotal
        FROM order_details od
        JOIN products p ON od.product_id = p.productid
        WHERE od.order_id = @order_id;
    ";

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@order_id", orderId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            flowPanelProducts.Controls.Clear(); // Clear old controls first
                            flowPanelProducts.AutoScroll = true; // ensure scrolling is enabled

                            bool any = false;
                            while (reader.Read())
                            {
                                any = true;
                                string name = reader.IsDBNull(0) ? "-" : reader.GetString(0);
                                int qty = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                decimal price = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
                                decimal total = reader.IsDBNull(3) ? qty * price : reader.GetDecimal(3);

                                var productCard = CreateProductCard(name, qty, price, total);
                                flowPanelProducts.Controls.Add(productCard);
                            }

                            if (!any)
                            {
                                // helpful placeholder so user sees no items found
                                var lbl = new Label
                                {
                                    Text = "No items found for this order.",
                                    AutoSize = true,
                                    Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Italic),
                                    Padding = new Padding(8),
                                    ForeColor = System.Drawing.Color.DimGray
                                };
                                flowPanelProducts.Controls.Add(lbl);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading order products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // ✅ Helper to make nice product panels
        private Panel CreateProductCard(string name, int qty, decimal price, decimal total)
        {
            Panel card = new Panel
            {
                Width = 250,
                Height = 80,
                BackColor = Color.WhiteSmoke,
                Margin = new Padding(8),
                Padding = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            Label lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(5, 5)
            };

            Label lblQty = new Label
            {
                Text = $"Qty: {qty}",
                Location = new Point(5, 35),
                AutoSize = true
            };

            Label lblPrice = new Label
            {
                Text = $"Price: {price:C}",
                Location = new Point(100, 35),
                AutoSize = true
            };

            Label lblTotal = new Label
            {
                Text = $"Total: {total:C}",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Location = new Point(5, 55),
                AutoSize = true
            };

            card.Controls.Add(lblName);
            card.Controls.Add(lblQty);
            card.Controls.Add(lblPrice);
            card.Controls.Add(lblTotal);

            return card;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
