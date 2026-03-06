using OOAD_Project.Domain;
using Guna.UI2.WinForms;
using System.Drawing;
using System.Windows.Forms;

namespace OOAD_Project.Patterns
{
    // ══════════════════════════════════════════════════════════════════════════
    //  STRATEGY INTERFACE
    // ══════════════════════════════════════════════════════════════════════════
    public interface IPaymentStrategy
    {
        bool ProcessPayment(decimal amount, Order order);
        string GetPaymentMethodName();
        void PrintReceipt(Order order);
        bool CanProcess(decimal amount);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CASH PAYMENT STRATEGY
    // ══════════════════════════════════════════════════════════════════════════
    public class CashPaymentStrategy : IPaymentStrategy
    {
        private decimal _received, _change;

        public bool ProcessPayment(decimal amount, Order order)
        {
            bool ok = false;

            // ── Form ──────────────────────────────────────────────────────
            var dlg = new Form
            {
                Text = "Cash Payment",
                Width = 460,
                Height = 460,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(30, 30, 45)
            };

            // Guna2 Panel as the rounded card
            var card = new Guna2Panel
            {
                Size = new Size(420, 420),
                Location = new Point(20, 20),
                BorderRadius = 16,
                FillColor = Color.FromArgb(40, 42, 60),
                BorderColor = Color.FromArgb(80, 80, 120),
                BorderThickness = 1
            };
            dlg.Controls.Add(card);

            // Title
            var lblTitle = new Guna2HtmlLabel
            {
                Text = "💵  CASH PAYMENT",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(212, 175, 95),
                AutoSize = true,
                Location = new Point(20, 20),
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTitle);

            // Order total label
            var lblTotalCaption = new Label
            {
                Text = "ORDER TOTAL",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(130, 130, 160),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 62)
            };
            card.Controls.Add(lblTotalCaption);

            var lblTotal = new Guna2HtmlLabel
            {
                Text = $"${amount:F2}",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.WhiteSmoke,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 80)
            };
            card.Controls.Add(lblTotal);

            // Separator
            var sep1 = new Guna2Separator
            {
                Size = new Size(380, 1),
                Location = new Point(20, 120),
                FillColor = Color.FromArgb(60, 60, 90)
            };
            card.Controls.Add(sep1);

            // Cash received label
            var lblCashCaption = new Label
            {
                Text = "CASH RECEIVED",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(130, 130, 160),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(30, 142)
            };
            card.Controls.Add(lblCashCaption);

            // Guna2 TextBox for cash input
            var txtCash = new Guna2TextBox
            {
                Size = new Size(380, 60),
                Location = new Point(20, 152),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.WhiteSmoke,
                FillColor = Color.FromArgb(50, 52, 75),
                BorderColor = Color.FromArgb(212, 175, 95),
                BorderRadius = 8,
                BorderThickness = 1,
                PlaceholderText = "0.00",
                TextAlign = HorizontalAlignment.Center
            };
            card.Controls.Add(txtCash);

            // Change due label
            var lblChangeCaption = new Label
            {
                Text = "CHANGE DUE",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(130, 130, 160),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 228)
            };
            card.Controls.Add(lblChangeCaption);

            var lblChange = new Guna2HtmlLabel
            {
                Text = "—",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(130, 130, 160),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 246)
            };
            card.Controls.Add(lblChange);

            // Live change calculation
            txtCash.TextChanged += (_, _) =>
            {
                if (decimal.TryParse(txtCash.Text, out decimal cash))
                {
                    decimal diff = cash - amount;
                    lblChange.Text = diff >= 0 ? $"${diff:F2}" : $"-${Math.Abs(diff):F2}";
                    lblChange.ForeColor = diff >= 0 ? Color.FromArgb(52, 211, 153) : Color.FromArgb(252, 100, 100);
                }
                else { lblChange.Text = "—"; lblChange.ForeColor = Color.FromArgb(130, 130, 160); }
            };

            // Only digits and decimal point
            txtCash.KeyPress += (_, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                    e.Handled = true;
            };

            // ── Buttons ───────────────────────────────────────────────────
            var btnConfirm = new Guna2Button
            {
                Text = "CONFIRM →",
                Size = new Size(240, 60),
                Location = new Point(20, 320),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 35),
                FillColor = Color.FromArgb(212, 175, 95),
                BorderRadius = 10,
                BorderThickness = 0
            };

            var btnCancel = new Guna2Button
            {
                Text = "CANCEL",
                Size = new Size(124, 60),
                Location = new Point(276, 320),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(252, 100, 100),
                FillColor = Color.Transparent,
                BorderColor = Color.FromArgb(252, 100, 100),
                BorderRadius = 10,
                BorderThickness = 1
            };

            btnConfirm.Click += (_, _) =>
            {
                if (!decimal.TryParse(txtCash.Text, out decimal cash))
                {
                    lblChange.Text = "⚠ Enter an amount";
                    lblChange.ForeColor = Color.FromArgb(252, 100, 100);
                    txtCash.Focus();
                    return;
                }
                if (cash < amount)
                {
                    lblChange.Text = $"⚠ Short  ${amount - cash:F2}";
                    lblChange.ForeColor = Color.FromArgb(252, 100, 100);
                    txtCash.Focus(); txtCash.SelectAll();
                    return;
                }
                _received = cash;
                _change = cash - amount;
                ok = true;
                dlg.DialogResult = DialogResult.OK;
            };
            btnCancel.Click += (_, _) => dlg.Close();

            card.Controls.AddRange(new Control[] { btnConfirm, btnCancel });

            // Draggable form
            bool drag = false; Point off = Point.Empty;
            dlg.MouseDown += (_, m) => { drag = true; off = m.Location; };
            dlg.MouseMove += (_, m) =>
            {
                if (drag) dlg.Location = new Point(dlg.Left + m.X - off.X, dlg.Top + m.Y - off.Y);
            };
            dlg.MouseUp += (_, _) => drag = false;

            // Close X button
            var btnX = new Guna2Button
            {
                Text = "✕",
                Size = new Size(32, 32),
                Location = new Point(dlg.Width - 52, 8),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(130, 130, 160),
                FillColor = Color.Transparent,
                BorderThickness = 0,
                BorderRadius = 8
            };
            btnX.Click += (_, _) => dlg.Close();
            dlg.Controls.Add(btnX);

            dlg.Shown += (_, _) => txtCash.Focus();
            dlg.ShowDialog();
            return ok;
        }

        public string GetPaymentMethodName() => "Cash";

        public void PrintReceipt(Order order) => ShowReceipt("💵  CASH RECEIPT",
            Color.FromArgb(52, 211, 153), new[]
            {
                ("ORDER ID",      $"#{order.OrderId}"),
                ("DATE",          DateTime.Now.ToString("yyyy-MM-dd  HH:mm")),
                ("TABLE",         order.TableId?.ToString() ?? "Takeaway"),
                ("TOTAL",         $"${order.TotalAmount:F2}"),
                ("CASH RECEIVED", $"${_received:F2}"),
                ("CHANGE GIVEN",  $"${_change:F2}"),
                ("METHOD",        "Cash"),
            });

        public bool CanProcess(decimal amount) => amount > 0;

        // ── Shared receipt dialog (Guna2 only) ────────────────────────────
        internal static void ShowReceipt(string title, Color accent, (string label, string value)[] rows)
        {
            int W = 460;
            int rowH = 46;
            int H = 160 + rows.Length * rowH + 70;

            var dlg = new Form
            {
                Text = string.Empty,
                Width = W,
                Height = H,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(30, 30, 45)
            };

            var card = new Guna2Panel
            {
                Size = new Size(W - 40, H - 40),
                Location = new Point(20, 20),
                BorderRadius = 16,
                FillColor = Color.FromArgb(40, 42, 60),
                BorderColor = Color.FromArgb(80, 80, 120),
                BorderThickness = 1
            };
            dlg.Controls.Add(card);

            var lblTitle = new Guna2HtmlLabel
            {
                Text = title,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = accent,
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 16)
            };
            card.Controls.Add(lblTitle);

            var sep = new Guna2Separator
            {
                Size = new Size(card.Width - 40, 1),
                Location = new Point(20, 50),
                FillColor = Color.FromArgb(60, 60, 90)
            };
            card.Controls.Add(sep);

            int y = 64;
            foreach (var (label, value) in rows)
            {
                bool hi = label is "TOTAL" or "CHANGE GIVEN";

                card.Controls.Add(new Label
                {
                    Text = label,
                    Font = new Font("Segoe UI", 7.5f),
                    ForeColor = Color.FromArgb(100, 100, 140),
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Location = new Point(20, y)
                });

                card.Controls.Add(new Guna2HtmlLabel
                {
                    Text = value,
                    Font = hi ? new Font("Segoe UI", 13, FontStyle.Bold) : new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    ForeColor = hi ? accent : Color.WhiteSmoke,
                    AutoSize = true,
                    BackColor = Color.Transparent,
                    Location = new Point(card.Width - 200, y)
                });
                y += rowH;
            }

            var btnDone = new Guna2Button
            {
                Text = "DONE  ✓",
                Size = new Size(card.Width - 40, 44),
                Location = new Point(20, y + 8),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(20, 20, 35),
                FillColor = accent,
                BorderRadius = 10,
                BorderThickness = 0
            };
            btnDone.Click += (_, _) => dlg.Close();
            card.Controls.Add(btnDone);

            bool drag = false; Point off = Point.Empty;
            dlg.MouseDown += (_, m) => { drag = true; off = m.Location; };
            dlg.MouseMove += (_, m) =>
            {
                if (drag) dlg.Location = new Point(dlg.Left + m.X - off.X, dlg.Top + m.Y - off.Y);
            };
            dlg.MouseUp += (_, _) => drag = false;

            dlg.ShowDialog();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  QR PAYMENT STRATEGY
    // ══════════════════════════════════════════════════════════════════════════
    public class QRPaymentStrategy : IPaymentStrategy
    {
        private string? _txId;

        public bool ProcessPayment(decimal amount, Order order)
        {
            bool ok = false;
            string qrData = $"PAY|OID={order.OrderId}|AMT={amount:F2}|T={DateTime.Now:yyyyMMddHHmmss}";

            const int W = 500;
            const int frameSize = 260;
            int H = 200 + frameSize + 120;

            var dlg = new Form
            {
                Text = string.Empty,
                Width = W,
                Height = H,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.None,
                BackColor = Color.FromArgb(30, 30, 45)
            };

            var card = new Guna2Panel
            {
                Size = new Size(W - 40, H - 40),
                Location = new Point(20, 20),
                BorderRadius = 16,
                FillColor = Color.FromArgb(40, 42, 60),
                BorderColor = Color.FromArgb(80, 80, 120),
                BorderThickness = 1
            };
            dlg.Controls.Add(card);

            // Title
            var lblTitle = new Guna2HtmlLabel
            {
                Text = "📱  QR PAYMENT",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 150, 255),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 18)
            };
            card.Controls.Add(lblTitle);

            var lblSub = new Label
            {
                Text = $"Total_Pay  •  ${amount:F2}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(130, 130, 160),
                AutoSize = true,
                BackColor = Color.Transparent,
                Location = new Point(20, 55)
            };
            card.Controls.Add(lblSub);

            var sep = new Guna2Separator
            {
                Size = new Size(card.Width - 40, 1),
                Location = new Point(20, 85),
                FillColor = Color.FromArgb(60, 60, 90)
            };
            card.Controls.Add(sep);

            // QR code PictureBox — standard WinForms control (no Guna equivalent)
            int frameX = (card.Width - frameSize) / 2;
            var pb = new PictureBox
            {
                Size = new Size(frameSize, frameSize),
                Location = new Point(frameX, 105),
                BackColor = Color.White,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None
            };
            try
            {
                using var gen = new QRCoder.QRCodeGenerator();
                using var data = gen.CreateQrCode(qrData, QRCoder.QRCodeGenerator.ECCLevel.Q);
                using var code = new QRCoder.QRCode(data);
                pb.Image = code.GetGraphic(6);
            }
            catch
            {
                pb.Controls.Add(new Label
                {
                    Text = "QR unavailable",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = Color.Gray
                });
            }
            card.Controls.Add(pb);

            // Hint
            var hint = new Label
            {
                Text = "Open your banking app  →  Scan  →  Approve",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(130, 130, 160),
                AutoSize = false,
                Size = new Size(card.Width - 40, 22),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 90 + frameSize + 30)
            };
            card.Controls.Add(hint);

            // Buttons
            int btnY = card.Height - 104;
            int btnW = card.Width - 40;
            int confirmW = (int)(btnW * 0.65);
            int cancelW = btnW - confirmW - 12;

            var btnConfirm = new Guna2Button
            {
                Text = "PAYMENT CONFIRMED",
                Size = new Size(confirmW, 65),
                Location = new Point(20, btnY),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                FillColor = Color.FromArgb(70, 150, 255),
                BorderRadius = 10,
                BorderThickness = 0
            };

            var btnCancel = new Guna2Button
            {
                Text = "CANCEL",
                Size = new Size(cancelW, 65),
                Location = new Point(20 + confirmW + 12, btnY),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(252, 100, 100),
                FillColor = Color.Transparent,
                BorderColor = Color.FromArgb(252, 100, 100),
                BorderRadius = 10,
                BorderThickness = 1
            };

            btnConfirm.Click += (_, _) =>
            {
                _txId = Guid.NewGuid().ToString("N")[..12].ToUpper();
                ok = true;
                dlg.DialogResult = DialogResult.OK;
            };
            btnCancel.Click += (_, _) => dlg.Close();

            card.Controls.AddRange(new Control[] { btnConfirm, btnCancel });

            bool drag = false; Point off = Point.Empty;
            dlg.MouseDown += (_, m) => { drag = true; off = m.Location; };
            dlg.MouseMove += (_, m) =>
            {
                if (drag) dlg.Location = new Point(dlg.Left + m.X - off.X, dlg.Top + m.Y - off.Y);
            };
            dlg.MouseUp += (_, _) => drag = false;

            var btnX = new Guna2Button
            {
                Text = "✕",
                Size = new Size(32, 32),
                Location = new Point(dlg.Width - 52, 8),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(130, 130, 160),
                FillColor = Color.Transparent,
                BorderThickness = 0,
                BorderRadius = 8
            };
            btnX.Click += (_, _) => dlg.Close();
            dlg.Controls.Add(btnX);

            dlg.ShowDialog();
            return ok;
        }

        public string GetPaymentMethodName() => "QR";

        public void PrintReceipt(Order order) => CashPaymentStrategy.ShowReceipt(
            "📱  QR RECEIPT", Color.FromArgb(70, 150, 255), new[]
            {
                ("ORDER ID",       $"#{order.OrderId}"),
                ("DATE",           DateTime.Now.ToString("yyyy-MM-dd  HH:mm")),
                ("TABLE",          order.TableId?.ToString() ?? "Takeaway"),
                ("TOTAL",          $"${order.TotalAmount:F2}"),
                ("TRANSACTION ID", _txId ?? "—"),
                ("METHOD",         "QR Code"),
            });

        public bool CanProcess(decimal amount) => amount > 0;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PAYMENT CONTEXT
    // ══════════════════════════════════════════════════════════════════════════
    public class PaymentContext
    {
        private IPaymentStrategy? _strategy;

        public void SetStrategy(IPaymentStrategy strategy)
        {
            _strategy = strategy;
            Console.WriteLine($"[PaymentContext] Strategy → {strategy.GetPaymentMethodName()}");
        }

        public bool ExecutePayment(decimal amount, Order order)
        {
            if (_strategy == null)
            {
                MessageBox.Show("Please select a payment method first.",
                    "No Method", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!_strategy.CanProcess(amount))
            {
                MessageBox.Show("Invalid payment amount.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            bool success = _strategy.ProcessPayment(amount, order);
            if (success) _strategy.PrintReceipt(order);
            return success;
        }

        public string GetCurrentMethod() => _strategy?.GetPaymentMethodName() ?? "None";
    }
}