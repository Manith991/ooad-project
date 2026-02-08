using Npgsql;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;

namespace OOAD_Project
{
    public partial class LoginForm : Form
    {
        private const string usernamePlaceholder = "Enter Username...";
        private const string passwordPlaceholder = "Enter Password...";
        private PictureBox eyeIcon;
        private bool isPasswordVisible = false;

        public LoginForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddEllipse(0, 0, pbProfile.Width - 1, pbProfile.Height - 1);
                pbProfile.Region = new Region(path);
            }

            pbProfile.BackColor = Color.White;
            pbProfile.SizeMode = PictureBoxSizeMode.StretchImage;

            txtUsername.Text = usernamePlaceholder;
            txtUsername.ForeColor = Color.Gray;
            txtUsername.BorderStyle = BorderStyle.None;
            AddBottomBorder(txtUsername);

            txtPassword.Text = passwordPlaceholder;
            txtPassword.ForeColor = Color.Gray;
            txtPassword.BorderStyle = BorderStyle.None;
            AddBottomBorder(txtPassword);

            this.AcceptButton = btnLogin;
            AddEyeIcon();
        }

        private void AddBottomBorder(TextBox textBox)
        {
            Panel borderPanel = new Panel
            {
                Height = 2,
                Width = textBox.Width,
                BackColor = Color.Gray,
                Location = new Point(textBox.Location.X, textBox.Location.Y + textBox.Height)
            };
            textBox.Parent.Controls.Add(borderPanel);
            borderPanel.BringToFront();
        }

        private void AddEyeIcon()
        {
            eyeIcon = new PictureBox
            {
                Size = new Size(24, 24),
                Location = new Point(txtPassword.Location.X + txtPassword.Width - 30,
                                   txtPassword.Location.Y + (txtPassword.Height - 24) / 2),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Cursor = Cursors.Hand,
                BackColor = Color.White
            };

            DrawEyeIcon(false);
            eyeIcon.Click += EyeIcon_Click;
            txtPassword.Parent.Controls.Add(eyeIcon);
            eyeIcon.BringToFront();
        }

        private void EyeIcon_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == passwordPlaceholder)
                return;

            isPasswordVisible = !isPasswordVisible;
            txtPassword.PasswordChar = isPasswordVisible ? '\0' : '●';
            DrawEyeIcon(isPasswordVisible);
        }

        private void DrawEyeIcon(bool isOpen)
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                Pen pen = new Pen(Color.Gray, 2);

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
            }
            eyeIcon.Image = bmp;
        }

        private void PbProfile_Paint(object sender, PaintEventArgs e)
        {
            const int borderThickness = 5;
            Color borderColor = Color.DarkGray;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(borderColor, borderThickness))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawEllipse(pen, 0, 0, pbProfile.Width - 1, pbProfile.Height - 1);
            }
        }

        private void TxtUsername_Enter(object sender, EventArgs e)
        {
            if (txtUsername.Text == usernamePlaceholder)
            {
                txtUsername.Text = "";
                txtUsername.ForeColor = Color.Black;
            }
        }

        private void TxtUsername_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                txtUsername.Text = usernamePlaceholder;
                txtUsername.ForeColor = Color.Gray;
            }
        }

        private void TxtPassword_Enter(object sender, EventArgs e)
        {
            if (txtPassword.Text == passwordPlaceholder)
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = Color.Black;
                txtPassword.PasswordChar = '●';
                isPasswordVisible = false;
                DrawEyeIcon(false);
            }
        }

        private void TxtPassword_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                txtPassword.PasswordChar = '\0';
                txtPassword.Text = passwordPlaceholder;
                txtPassword.ForeColor = Color.Gray;
                isPasswordVisible = false;
                DrawEyeIcon(false);
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string enteredUsername = txtUsername.Text;
            string enteredPassword = txtPassword.Text;

            if (enteredUsername == usernamePlaceholder || string.IsNullOrWhiteSpace(enteredUsername))
            {
                MessageBox.Show("Please enter your username.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (enteredPassword == passwordPlaceholder || string.IsNullOrWhiteSpace(enteredPassword))
            {
                MessageBox.Show("Please enter your password.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            try
            {
                using (var conn = Database.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT username, role FROM users WHERE username = @username AND password = @password";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", enteredUsername);
                        cmd.Parameters.AddWithValue("@password", HashPassword(enteredPassword));

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string loggedInUser = reader.GetString(0);
                                string role = reader.GetString(1);

                                MessageBox.Show($"Welcome back, {loggedInUser}!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                MenuForm menu = new MenuForm(loggedInUser, role);

                                this.Hide();
                                menu.FormClosed += (s, args) =>
                                {
                                    txtUsername.Text = usernamePlaceholder;
                                    txtUsername.ForeColor = Color.Gray;
                                    txtPassword.Text = passwordPlaceholder;
                                    txtPassword.ForeColor = Color.Gray;
                                    txtPassword.PasswordChar = '\0';
                                    this.Show();
                                };

                                menu.Show();
                            }
                            else
                            {
                                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LblRegister_Click(object sender, EventArgs e)
        {
            RegisterForm registerForm = new RegisterForm();
            registerForm.FormClosed += (s, args) => this.Close();
            registerForm.Show();
            this.Hide();
        }

        private void lblForgotPassword_Click(object sender, EventArgs e)
        {
            FormResetPass forgotForm = new FormResetPass();
            forgotForm.StartPosition = FormStartPosition.CenterScreen; // ✅ Center the form
            forgotForm.Show();
        }

    }
}
