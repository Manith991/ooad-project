namespace OOAD_Project
{
    partial class StaffForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StaffForm));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            panel1 = new Panel();
            dgvStaff = new DataGridView();
            colNo = new DataGridViewTextBoxColumn();
            colStaff = new DataGridViewTextBoxColumn();
            colRole = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colImage = new DataGridViewImageColumn();
            colEdit = new DataGridViewImageColumn();
            colDelete = new DataGridViewImageColumn();
            panel2 = new Panel();
            btnAdd = new Guna.UI2.WinForms.Guna2Button();
            lblStaff = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvStaff).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(dgvStaff);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 124);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(35, 0, 35, 35);
            panel1.Size = new Size(1430, 846);
            panel1.TabIndex = 10;
            // 
            // dgvStaff
            // 
            dgvStaff.AllowUserToAddRows = false;
            dgvStaff.AllowUserToDeleteRows = false;
            dgvStaff.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvStaff.BackgroundColor = Color.White;
            dgvStaff.BorderStyle = BorderStyle.None;
            dgvStaff.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.Silver;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 12F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = Color.Silver;
            dataGridViewCellStyle1.SelectionForeColor = Color.Black;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvStaff.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvStaff.ColumnHeadersHeight = 70;
            dgvStaff.Columns.AddRange(new DataGridViewColumn[] { colNo, colStaff, colRole, colStatus, colImage, colEdit, colDelete });
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = Color.White;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.False;
            dgvStaff.DefaultCellStyle = dataGridViewCellStyle2;
            dgvStaff.Dock = DockStyle.Fill;
            dgvStaff.EnableHeadersVisualStyles = false;
            dgvStaff.GridColor = Color.Black;
            dgvStaff.Location = new Point(35, 0);
            dgvStaff.MultiSelect = false;
            dgvStaff.Name = "dgvStaff";
            dgvStaff.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.Silver;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 11F);
            dataGridViewCellStyle3.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.True;
            dgvStaff.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvStaff.RowHeadersVisible = false;
            dgvStaff.RowHeadersWidth = 62;
            dgvStaff.RowTemplate.Height = 70;
            dgvStaff.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvStaff.Size = new Size(1360, 811);
            dgvStaff.TabIndex = 7;
            //dgvStaff.CellContentClick += dgvStaff_CellContentClick;
            // 
            // colNo
            // 
            colNo.FillWeight = 21.5810242F;
            colNo.HeaderText = "No.";
            colNo.MinimumWidth = 8;
            colNo.Name = "colNo";
            colNo.ReadOnly = true;
            // 
            // colStaff
            // 
            colStaff.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colStaff.FillWeight = 271.0407F;
            colStaff.HeaderText = "Staff Name";
            colStaff.MinimumWidth = 8;
            colStaff.Name = "colStaff";
            colStaff.ReadOnly = true;
            // 
            // colRole
            // 
            colRole.FillWeight = 36.6760979F;
            colRole.HeaderText = "Role";
            colRole.MinimumWidth = 8;
            colRole.Name = "colRole";
            colRole.ReadOnly = true;
            // 
            // colStatus
            // 
            colStatus.FillWeight = 37.4344139F;
            colStatus.HeaderText = "Status";
            colStatus.MinimumWidth = 8;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            // 
            // colImage
            // 
            colImage.FillWeight = 21.6094475F;
            colImage.HeaderText = "Image";
            colImage.Image = (Image)resources.GetObject("colImage.Image");
            colImage.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colImage.MinimumWidth = 8;
            colImage.Name = "colImage";
            colImage.ReadOnly = true;
            // 
            // colEdit
            // 
            colEdit.FillWeight = 15.7088919F;
            colEdit.HeaderText = "";
            colEdit.Image = (Image)resources.GetObject("colEdit.Image");
            colEdit.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colEdit.MinimumWidth = 8;
            colEdit.Name = "colEdit";
            colEdit.ReadOnly = true;
            // 
            // colDelete
            // 
            colDelete.FillWeight = 15.7900839F;
            colDelete.HeaderText = "";
            colDelete.Image = (Image)resources.GetObject("colDelete.Image");
            colDelete.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colDelete.MinimumWidth = 8;
            colDelete.Name = "colDelete";
            colDelete.ReadOnly = true;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(btnAdd);
            panel2.Controls.Add(lblStaff);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(1430, 124);
            panel2.TabIndex = 11;
            // 
            // btnAdd
            // 
            btnAdd.BorderRadius = 10;
            btnAdd.BorderThickness = 1;
            btnAdd.CustomizableEdges = customizableEdges1;
            btnAdd.DisabledState.BorderColor = Color.DarkGray;
            btnAdd.DisabledState.CustomBorderColor = Color.DarkGray;
            btnAdd.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnAdd.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnAdd.FillColor = Color.Black;
            btnAdd.Font = new Font("Segoe UI", 9F);
            btnAdd.ForeColor = Color.White;
            btnAdd.Image = (Image)resources.GetObject("btnAdd.Image");
            btnAdd.ImageSize = new Size(60, 60);
            btnAdd.Location = new Point(34, 26);
            btnAdd.Margin = new Padding(0);
            btnAdd.Name = "btnAdd";
            btnAdd.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnAdd.Size = new Size(70, 70);
            btnAdd.TabIndex = 5;
            // 
            // lblStaff
            // 
            lblStaff.AutoSize = true;
            lblStaff.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            lblStaff.Location = new Point(107, 42);
            lblStaff.Name = "lblStaff";
            lblStaff.Size = new Size(76, 38);
            lblStaff.TabIndex = 6;
            lblStaff.Text = "Staff";
            // 
            // StaffForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1430, 970);
            Controls.Add(panel1);
            Controls.Add(panel2);
            FormBorderStyle = FormBorderStyle.None;
            Name = "StaffForm";
            Text = "StaffForm";
            WindowState = FormWindowState.Maximized;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvStaff).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private DataGridView dgvStaff;
        private Panel panel2;
        private Guna.UI2.WinForms.Guna2Button btnAdd;
        private Label lblStaff;
        private DataGridViewTextBoxColumn colNo;
        private DataGridViewTextBoxColumn colStaff;
        private DataGridViewTextBoxColumn colRole;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewImageColumn colImage;
        private DataGridViewImageColumn colEdit;
        private DataGridViewImageColumn colDelete;
    }
}