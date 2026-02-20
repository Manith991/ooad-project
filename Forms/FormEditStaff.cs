using OOAD_Project.Domain;
using OOAD_Project.Patterns.Command;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project
{
    /// <summary>
    /// COMMAND PATTERN applied:
    ///   - Save triggers UpdateStaffCommand through CommandInvoker,
    ///     so the edit is tracked in the undo/redo stack.
    /// REPOSITORY PATTERN applied:
    ///   - Persistence delegated to IRepository&lt;User&gt; (UserRepository)
    ///     instead of raw NpgsqlCommand calls.
    /// </summary>
    public partial class FormEditStaff : Form
    {
        private readonly int _userId;
        private string? _currentImagePath;

        // Public properties read by the caller after ShowDialog()
        public string CurrentImagePath => _currentImagePath ?? string.Empty;
        public string StaffNameValue => txtStaff.Text.Trim();
        public string RoleValue => cbRole.Text;
        public string StatusValue => cbStatus.Text;

        // REPOSITORY PATTERN
        private readonly IRepository<User> _userRepository;

        // COMMAND PATTERN
        private readonly CommandInvoker _invoker;

        public FormEditStaff(
            int userId,
            string name,
            string role,
            string status,
            string? imagePath,
            CommandInvoker? invoker = null,
            IRepository<User>? userRepository = null)
        {
            InitializeComponent();

            _userId = userId;
            txtStaff.Text = name;
            cbRole.Text = role;
            cbStatus.Text = status;
            _currentImagePath = imagePath;

            // Allow dependency injection; fall back to defaults
            _invoker = invoker ?? new CommandInvoker();
            _userRepository = userRepository ?? new UserRepository();

            LoadImage(imagePath);
        }

        // ── Load image safely ─────────────────────────────────────────────
        private void LoadImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            string[] possiblePaths =
            {
                imagePath,
                Path.Combine(Application.StartupPath, "Resources", imagePath),
                Path.Combine(Application.StartupPath, "Resources", imagePath + ".png"),
                Path.Combine(Application.StartupPath, "Resources", imagePath + ".jpg"),
                Path.Combine(Application.StartupPath, "Resources", imagePath + ".jpeg")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    pbImage.Image = Image.FromFile(path);
                    return;
                }
            }
        }

        // ── Browse for new image ──────────────────────────────────────────
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg" };
            if (ofd.ShowDialog() != DialogResult.OK) return;

            string selectedPath = ofd.FileName;

            using (var fs = new FileStream(selectedPath, FileMode.Open, FileAccess.Read))
                pbImage.Image = Image.FromStream(fs);

            string resourcesFolder = Path.Combine(Application.StartupPath, "Resources");
            Directory.CreateDirectory(resourcesFolder);

            string fileNameOnly = Path.GetFileNameWithoutExtension(selectedPath);
            string extension = Path.GetExtension(selectedPath);
            string destPath = Path.Combine(resourcesFolder, Path.GetFileName(selectedPath));

            int counter = 1;
            while (File.Exists(destPath))
            {
                destPath = Path.Combine(resourcesFolder, $"{fileNameOnly}_{counter}{extension}");
                counter++;
            }

            File.Copy(selectedPath, destPath, overwrite: false);
            _currentImagePath = Path.GetFileName(destPath);
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();

        // ── Save – COMMAND PATTERN ────────────────────────────────────────
        private void btnSave_Click(object sender, EventArgs e)
        {
            string newName = txtStaff.Text.Trim();
            string newRole = cbRole.Text;
            string newStatus = cbStatus.Text;

            if (string.IsNullOrEmpty(newName) || string.IsNullOrEmpty(newRole))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build updated domain object
            var updatedUser = new User
            {
                Id = _userId,
                Name = newName,
                Role = newRole,
                Status = newStatus,
                Image = _currentImagePath
            };

            try
            {
                // COMMAND PATTERN: wrap the update so it can be undone
                var command = new UpdateStaffCommand(updatedUser, _userRepository);
                _invoker.ExecuteCommand(command);

                MessageBox.Show("Staff information updated successfully.", "Saved",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating staff: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}