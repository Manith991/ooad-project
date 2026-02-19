using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OOAD_Project.Patterns.Command;

namespace OOAD_Project.Patterns.TemplateMethod
{
    /// <summary>
    /// TEMPLATE METHOD PATTERN – Base CRUD form.
    /// Defines the skeleton algorithm; subclasses fill in the steps.
    ///
    /// Skeleton:
    ///   InitializeForm()
    ///     ├── SetupEventHandlers()   (wires grid + add button)
    ///     ├── LoadData()             ← ABSTRACT – each form loads its own data
    ///     ├── RestrictActionsByRole()← virtual  – hides Edit/Delete for non-admins
    ///     └── ShowCommandStatus()    (logs undo/redo counts)
    ///
    ///   CellContentClick             (routes to OnEdit / OnDelete)
    ///     ├── OnEdit(entity)         ← ABSTRACT
    ///     └── OnDelete(id)           ← ABSTRACT
    ///
    ///   ProcessCmdKey                (Ctrl+Z Undo / Ctrl+Y Redo) – built-in
    /// </summary>
    public abstract partial class BaseCrudForm<T> : Form where T : class
    {
        // ── Controls assigned by subclass constructor before InitializeForm() ──
        protected DataGridView dataGridView = null!;
        protected Button btnAdd = null!;

        // ── Shared infrastructure ──────────────────────────────────────────
        protected readonly string userRole;
        protected readonly CommandInvoker commandInvoker;

        protected BaseCrudForm(string userRole)
        {
            this.userRole = userRole;
            this.commandInvoker = new CommandInvoker(maxHistorySize: 50);
        }

        // ══════════════════════════════════════════════════════════════════
        // TEMPLATE METHOD – main skeleton (call from subclass constructor)
        // ══════════════════════════════════════════════════════════════════
        protected void InitializeForm()
        {
            SetupEventHandlers();
            LoadData();
            RestrictActionsByRole();
            ShowCommandStatus();
        }

        // ══════════════════════════════════════════════════════════════════
        // ABSTRACT STEPS – must be implemented by every subclass
        // ══════════════════════════════════════════════════════════════════

        /// <summary>Pull records from repository and populate the grid.</summary>
        protected abstract void LoadData();

        /// <summary>Show edit dialog and persist changes via a Command.</summary>
        protected abstract void OnEdit(T entity);

        /// <summary>Confirm and delete the record via a Command.</summary>
        protected abstract void OnDelete(int id);

        /// <summary>Build a domain object from the selected DataGridView row.</summary>
        protected abstract T GetEntityFromRow(DataGridViewRow row);

        /// <summary>Return the primary key stored in the selected row.</summary>
        protected abstract int GetEntityId(DataGridViewRow row);

        // ══════════════════════════════════════════════════════════════════
        // VIRTUAL STEPS – subclasses may override if needed
        // ══════════════════════════════════════════════════════════════════

        /// <summary>Wire the DataGridView cell-click and Add button.</summary>
        protected virtual void SetupEventHandlers()
        {
            if (dataGridView != null)
            {
                dataGridView.CellContentClick -= DataGridView_CellContentClick;
                dataGridView.CellContentClick += DataGridView_CellContentClick;
            }

            if (btnAdd != null)
            {
                btnAdd.Click -= OnAddClick;
                btnAdd.Click += OnAddClick;
            }
        }

        /// <summary>Hide Edit/Delete columns and disable Add for non-admins.</summary>
        protected virtual void RestrictActionsByRole()
        {
            if (userRole == "(admin)") return;

            if (btnAdd != null) btnAdd.Enabled = false;

            if (dataGridView != null)
            {
                if (dataGridView.Columns.Contains("colEdit"))
                    dataGridView.Columns["colEdit"].Visible = false;
                if (dataGridView.Columns.Contains("colDelete"))
                    dataGridView.Columns["colDelete"].Visible = false;
            }
        }

        /// <summary>Handle Add button – subclasses override to show their dialog.</summary>
        protected virtual void OnAddClick(object? sender, EventArgs e)
        {
            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admins can add items.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ══════════════════════════════════════════════════════════════════
        // CONCRETE STEPS – same for every form, no override needed
        // ══════════════════════════════════════════════════════════════════

        /// <summary>Routes Edit/Delete cell clicks to the correct abstract step.</summary>
        private void DataGridView_CellContentClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dataGridView.Columns[e.ColumnIndex].Name;
            if (colName != "colEdit" && colName != "colDelete") return;

            if (userRole != "(admin)")
            {
                MessageBox.Show("Only admins can modify or delete items.", "Access Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow row = dataGridView.Rows[e.RowIndex];

            if (colName == "colEdit")
            {
                T entity = GetEntityFromRow(row);
                if (entity != null) OnEdit(entity);
            }
            else
            {
                int id = GetEntityId(row);
                if (id < 0) return;

                var confirm = MessageBox.Show(
                    "Are you sure you want to delete this item?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                    OnDelete(id);
            }
        }

        // ── Undo / Redo – built-in for every subclass ─────────────────────
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.Z)) { PerformUndo(); return true; }
            if (keyData == (Keys.Control | Keys.Y)) { PerformRedo(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void PerformUndo()
        {
            if (!commandInvoker.CanUndo)
            {
                MessageBox.Show("Nothing to undo.", "Undo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string desc = commandInvoker.GetUndoDescription();
                commandInvoker.Undo();
                LoadData();
                MessageBox.Show($"Undid: {desc}", "Undo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Undo failed: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerformRedo()
        {
            if (!commandInvoker.CanRedo)
            {
                MessageBox.Show("Nothing to redo.", "Redo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                string desc = commandInvoker.GetRedoDescription();
                commandInvoker.Redo();
                LoadData();
                MessageBox.Show($"Redid: {desc}", "Redo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Redo failed: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────
        protected void ShowCommandStatus() =>
            Console.WriteLine($"[{GetType().Name}] Undo:{commandInvoker.UndoCount} Redo:{commandInvoker.RedoCount}");

        protected Image? LoadImage(string? imagePath, string subfolder = "")
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
            {
                try { if (File.Exists(p)) return Image.FromFile(p); }
                catch { /* skip */ }
            }

            return GetDefaultImage();
        }

        protected Image? GetDefaultImage()
        {
            string def = Path.Combine(Application.StartupPath, "Resources", "no_image.png");
            return File.Exists(def) ? Image.FromFile(def) : null;
        }
    }
}