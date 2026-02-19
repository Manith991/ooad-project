namespace OOAD_Project
{
    partial class TableForm
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TableForm));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            label1 = new Label();
            btnAdd = new Guna.UI2.WinForms.Guna2Button();
            panel2 = new Panel();
            dgvTable = new DataGridView();
            colNo = new DataGridViewTextBoxColumn();
            colTable = new DataGridViewTextBoxColumn();
            colCapacity = new DataGridViewTextBoxColumn();
            colStatus = new DataGridViewTextBoxColumn();
            colEdit = new DataGridViewImageColumn();
            colDelete = new DataGridViewImageColumn();
            panel1 = new Panel();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTable).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            label1.Location = new Point(107, 42);
            label1.Name = "label1";
            label1.Size = new Size(148, 38);
            label1.TabIndex = 6;
            label1.Text = "New Table";
            // 
            // btnAdd
            // 
            btnAdd.BorderRadius = 10;
            btnAdd.BorderThickness = 1;
            btnAdd.CustomizableEdges = customizableEdges3;
            btnAdd.DisabledState.BorderColor = Color.DarkGray;
            btnAdd.DisabledState.CustomBorderColor = Color.DarkGray;
            btnAdd.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnAdd.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnAdd.FillColor = Color.Blue;
            btnAdd.Font = new Font("Segoe UI", 9F);
            btnAdd.ForeColor = Color.White;
            btnAdd.Image = (Image)resources.GetObject("btnAdd.Image");
            btnAdd.ImageSize = new Size(60, 60);
            btnAdd.Location = new Point(34, 26);
            btnAdd.Margin = new Padding(0);
            btnAdd.Name = "btnAdd";
            btnAdd.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnAdd.Size = new Size(70, 70);
            btnAdd.TabIndex = 5;
            //btnAdd.Click += btnAdd_Click;
            // 
            // panel2
            // 
            panel2.BackColor = Color.White;
            panel2.Controls.Add(btnAdd);
            panel2.Controls.Add(label1);
            panel2.Dock = DockStyle.Top;
            panel2.Location = new Point(0, 0);
            panel2.Name = "panel2";
            panel2.Size = new Size(1430, 124);
            panel2.TabIndex = 9;
            // 
            // dgvTable
            // 
            dgvTable.AllowUserToAddRows = false;
            dgvTable.AllowUserToDeleteRows = false;
            dgvTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvTable.BackgroundColor = Color.White;
            dgvTable.BorderStyle = BorderStyle.None;
            dgvTable.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.Silver;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 12F);
            dataGridViewCellStyle4.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = Color.Silver;
            dataGridViewCellStyle4.SelectionForeColor = Color.Black;
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.True;
            dgvTable.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            dgvTable.ColumnHeadersHeight = 70;
            dgvTable.Columns.AddRange(new DataGridViewColumn[] { colNo, colTable, colCapacity, colStatus, colEdit, colDelete });
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = SystemColors.Window;
            dataGridViewCellStyle5.Font = new Font("Segoe UI", 10F);
            dataGridViewCellStyle5.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle5.SelectionForeColor = Color.White;
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.False;
            dgvTable.DefaultCellStyle = dataGridViewCellStyle5;
            dgvTable.Dock = DockStyle.Fill;
            dgvTable.EnableHeadersVisualStyles = false;
            dgvTable.GridColor = Color.Black;
            dgvTable.Location = new Point(35, 0);
            dgvTable.MultiSelect = false;
            dgvTable.Name = "dgvTable";
            dgvTable.ReadOnly = true;
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = Color.Silver;
            dataGridViewCellStyle6.Font = new Font("Segoe UI", 11F);
            dataGridViewCellStyle6.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = DataGridViewTriState.True;
            dgvTable.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            dgvTable.RowHeadersVisible = false;
            dgvTable.RowHeadersWidth = 62;
            dgvTable.RowTemplate.Height = 70;
            dgvTable.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTable.Size = new Size(1360, 811);
            dgvTable.TabIndex = 7;
            //dgvTable.CellContentClick += dgvTable_CellContentClick;
            // 
            // colNo
            // 
            colNo.FillWeight = 14.0979729F;
            colNo.HeaderText = "No.";
            colNo.MinimumWidth = 8;
            colNo.Name = "colNo";
            colNo.ReadOnly = true;
            // 
            // colTable
            // 
            colTable.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colTable.FillWeight = 235.17688F;
            colTable.HeaderText = "Table Name";
            colTable.MinimumWidth = 8;
            colTable.Name = "colTable";
            colTable.ReadOnly = true;
            // 
            // colCapacity
            // 
            colCapacity.FillWeight = 23.471571F;
            colCapacity.HeaderText = "Capacity";
            colCapacity.MinimumWidth = 8;
            colCapacity.Name = "colCapacity";
            colCapacity.ReadOnly = true;
            // 
            // colStatus
            // 
            colStatus.FillWeight = 23.532732F;
            colStatus.HeaderText = "Status";
            colStatus.MinimumWidth = 8;
            colStatus.Name = "colStatus";
            colStatus.ReadOnly = true;
            // 
            // colEdit
            // 
            colEdit.FillWeight = 11.7769127F;
            colEdit.HeaderText = "";
            colEdit.Image = (Image)resources.GetObject("colEdit.Image");
            colEdit.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colEdit.MinimumWidth = 8;
            colEdit.Name = "colEdit";
            colEdit.ReadOnly = true;
            // 
            // colDelete
            // 
            colDelete.FillWeight = 11.7845125F;
            colDelete.HeaderText = "";
            colDelete.Image = (Image)resources.GetObject("colDelete.Image");
            colDelete.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colDelete.MinimumWidth = 8;
            colDelete.Name = "colDelete";
            colDelete.ReadOnly = true;
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(dgvTable);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 124);
            panel1.Name = "panel1";
            panel1.Padding = new Padding(35, 0, 35, 35);
            panel1.Size = new Size(1430, 846);
            panel1.TabIndex = 8;
            // 
            // TableForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1430, 970);
            Controls.Add(panel1);
            Controls.Add(panel2);
            FormBorderStyle = FormBorderStyle.None;
            Name = "TableForm";
            Text = "TableForm";
            WindowState = FormWindowState.Maximized;
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvTable).EndInit();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Guna.UI2.WinForms.Guna2Button btnAdd;
        private Panel panel2;
        private DataGridView dgvTable;
        private DataGridViewTextBoxColumn colNo;
        private DataGridViewTextBoxColumn colTable;
        private DataGridViewTextBoxColumn colCapacity;
        private DataGridViewTextBoxColumn colStatus;
        private DataGridViewImageColumn colEdit;
        private DataGridViewImageColumn colDelete;
        private Panel panel1;
    }
}