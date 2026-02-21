using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project
{
    /// <summary>
    /// REPOSITORY PATTERN applied:
    ///   - User credential lookup delegated to UserRepository.GetByCredentials()
    ///     instead of raw NpgsqlCommand inside the form.
    ///
    /// No Command / Observer / Strategy pattern is needed here – the form
    /// only authenticates and navigates; it performs no data mutations.
    /// </summary>
    public partial class LoginForm : Form
    {
        private const string UsernamePlaceholder = "Enter Username...";
        private const string PasswordPlaceholder = "Enter Password...";

        private PictureBox _eyeIcon;
        private bool _isPasswordVisible = false;

        // REPOSITORY PATTERN
        private readonly UserRepository _userRepository;

        public LoginForm(UserRepository? userRepository = null)
        {
            InitializeComponent();
            _userRepository = userRepository ?? new UserRepository();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, pbProfile.Width - 1, pbProfile.Height - 1);
                pbProfile.Region = new Region(path);
            }

            pbProfile.BackColor = Color.White;
            pbProfile.SizeMode = PictureBoxSizeMode.StretchImage;

            txtUsername.Text = UsernamePlaceholder;
            txtUsername.ForeColor = Color.Gray;
            txtUsername.BorderStyle = BorderStyle.None;
            AddBottomBorder(txtUsername);

            txtPassword.Text = PasswordPlaceholder;
            txtPassword.ForeColor = Color.Gray;
            txtPassword.BorderStyle = BorderStyle.None;
            AddBottomBorder(txtPassword);

            this.AcceptButton = btnLogin;
            AddEyeIcon();
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

        private void AddEyeIcon()
        {
            _eyeIcon = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(
                    txtPassword.Location.X + txtPassword.Width - 30,
                    txtPassword.Location.Y + (txtPassword.Height - 24) / 2),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Cursor = Cursors.Hand,
                BackColor = Color.White
            };

            DrawEyeIcon(false);
            _eyeIcon.Click += EyeIcon_Click;
            txtPassword.Parent!.Controls.Add(_eyeIcon);
            _eyeIcon.BringToFront();
        }

        private void EyeIcon_Click(object? sender, EventArgs e)
        {
            if (txtPassword.Text == PasswordPlaceholder) return;

            _isPasswordVisible = !_isPasswordVisible;
            txtPassword.PasswordChar = _isPasswordVisible ? '\0' : '●';
            DrawEyeIcon(_isPasswordVisible);
        }

        private void DrawEyeIcon(bool isOpen)
        {
            var bmp = new Bitmap(24, 24);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            var pen = new Pen(Color.Gray, 2);

            if (isOpen)
            {
                g.DrawEllipse(pen, 2, 8, 20, 10);
                g.FillEllipse(Brushes.Gray, 9, 10, 6, 6);
            }
            else
            {
                g.DrawEllipse(pen, 2, 8, 20, 10);
                g.DrawLine(pen, 3, 20, 21, 6);
            }
            _eyeIcon.Image = bmp;
        }

        private void PbProfile_Paint(object sender, PaintEventArgs e)
        {
            const int borderThickness = 5;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(Color.DarkGray, borderThickness);
            pen.Alignment = PenAlignment.Inset;
            e.Graphics.DrawEllipse(pen, 0, 0, pbProfile.Width - 1, pbProfile.Height - 1);
        }

        // ── Placeholder events ────────────────────────────────────────────

        private void TxtUsername_Enter(object sender, EventArgs e)
        {
            if (txtUsername.Text == UsernamePlaceholder)
            {
                txtUsername.Text = "";
                txtUsername.ForeColor = Color.Black;
            }
        }

        private void TxtUsername_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = UsernamePlaceholder;
                txtUsername.ForeColor = Color.Gray;
            }
        }

        private void TxtPassword_Enter(object sender, EventArgs e)
        {
            if (txtPassword.Text == PasswordPlaceholder)
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = Color.Black;
                txtPassword.PasswordChar = '●';
                _isPasswordVisible = false;
                DrawEyeIcon(false);
            }
        }

        private void TxtPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtPassword.PasswordChar = '\0';
                txtPassword.Text = PasswordPlaceholder;
                txtPassword.ForeColor = Color.Gray;
                _isPasswordVisible = false;
                DrawEyeIcon(false);
            }
        }

        // ── Password hashing (unchanged) ──────────────────────────────────

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder();
            foreach (byte b in bytes) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }

        // ── Login – REPOSITORY PATTERN ────────────────────────────────────

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string enteredUsername = txtUsername.Text;
            string enteredPassword = txtPassword.Text;

            if (enteredUsername == UsernamePlaceholder || string.IsNullOrWhiteSpace(enteredUsername))
            {
                MessageBox.Show("Please enter your username.", "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (enteredPassword == PasswordPlaceholder || string.IsNullOrWhiteSpace(enteredPassword))
            {
                MessageBox.Show("Please enter your password.", "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                // REPOSITORY PATTERN: credential lookup via repository
                User? user = _userRepository.GetByCredentials(
                    enteredUsername, HashPassword(enteredPassword));

                if (user != null)
                {
                    MessageBox.Show($"Welcome back, {user.Username}!", "Login Successful",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var menu = new MenuForm(user.Username!, user.Role);
                    this.Hide();

                    menu.FormClosed += (s, args) =>
                    {
                        // Reset fields and show login again
                        txtUsername.Text = UsernamePlaceholder;
                        txtUsername.ForeColor = Color.Gray;
                        txtPassword.Text = PasswordPlaceholder;
                        txtPassword.ForeColor = Color.Gray;
                        txtPassword.PasswordChar = '\0';
                        this.Show();
                    };

                    menu.Show();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Login Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LblRegister_Click(object sender, EventArgs e)
        {
            var registerForm = new RegisterForm();
            registerForm.FormClosed += (s, args) => this.Close();
            registerForm.Show();
            this.Hide();
        }

        private void lblForgotPassword_Click(object sender, EventArgs e)
        {
            var forgotForm = new FormResetPass(_userRepository)
            {
                StartPosition = FormStartPosition.CenterScreen
            };
            forgotForm.Show();
        }
    }
}