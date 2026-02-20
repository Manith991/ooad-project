using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project
{
    /// <summary>
    /// REPOSITORY PATTERN applied:
    ///   - User lookup delegated to IRepository&lt;User&gt; (UserRepository)
    ///     instead of a raw NpgsqlCommand inside the form.
    ///
    /// No Command/Observer pattern needed – this form only validates an
    /// e-mail and navigates to FormNewPass; it performs no data mutations.
    /// </summary>
    public partial class FormResetPass : Form
    {
        private const string EmailPlaceholder = "Enter Email...";

        // REPOSITORY PATTERN
        private readonly UserRepository _userRepository;

        public FormResetPass(UserRepository? userRepository = null)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;

            _userRepository = userRepository ?? new UserRepository();

            // Placeholder setup
            txtEmail.Text = EmailPlaceholder;
            txtEmail.ForeColor = Color.Gray;
            txtEmail.BorderStyle = BorderStyle.None;
            AddBottomBorder(txtEmail);

            this.AcceptButton = btnReset;
        }

        private void FormResetPass_Load(object sender, EventArgs e)
        {
            // Re-apply placeholder in case InitializeComponent overwrites it
            txtEmail.Text = EmailPlaceholder;
            txtEmail.ForeColor = Color.Gray;
            txtEmail.BorderStyle = BorderStyle.None;
            AddBottomBorder(txtEmail);
        }

        private void AddBottomBorder(TextBox textBox)
        {
            var borderPanel = new Panel
            {
                Height = 2,
                Width = textBox.Width,
                BackColor = Color.Gray,
                Location = new Point(textBox.Location.X, textBox.Location.Y + textBox.Height)
            };
            textBox.Parent!.Controls.Add(borderPanel);
            borderPanel.BringToFront();
        }

        private void txtEmail_Enter(object sender, EventArgs e)
        {
            if (txtEmail.Text == EmailPlaceholder)
            {
                txtEmail.Text = "";
                txtEmail.ForeColor = Color.Black;
            }
        }

        private void txtEmail_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.Text = EmailPlaceholder;
                txtEmail.ForeColor = Color.Gray;
            }
        }

        // ── Reset button – REPOSITORY PATTERN ────────────────────────────
        private void btnReset_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();

            if (email == EmailPlaceholder || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Please enter your email.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // REPOSITORY PATTERN: delegate lookup to the repository
                User? user = _userRepository.GetByEmail(email);

                if (user != null &&
                    string.Equals(user.Status, "active", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Email verified! Proceed to set your new password.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var newPassForm = new FormNewPass(email)
                    {
                        StartPosition = FormStartPosition.CenterScreen
                    };
                    newPassForm.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Email not found or inactive.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
    }
}