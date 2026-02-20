
namespace OOAD_Project
{
    partial class FormEditStaff
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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEditStaff));
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            label4 = new Label();
            label3 = new Label();
            cbStatus = new ComboBox();
            btnBrowse = new Button();
            txtStaff = new TextBox();
            label2 = new Label();
            panel2 = new Panel();
            btnClose = new Guna.UI2.WinForms.Guna2Button();
            btnSave = new Guna.UI2.WinForms.Guna2Button();
            panel1 = new Panel();
            guna2Button1 = new Guna.UI2.WinForms.Guna2Button();
            lblProduct = new Label();
            cbRole = new ComboBox();
            pbImage = new PictureBox();
            panel2.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbImage).BeginInit();
            SuspendLayout();
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 11F);
            label4.Location = new Point(22, 211);
            label4.Name = "label4";
            label4.Size = new Size(60, 30);
            label4.TabIndex = 24;
            label4.Text = "Role:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 11F);
            label3.Location = new Point(22, 299);
            label3.Name = "label3";
            label3.Size = new Size(75, 30);
            label3.TabIndex = 23;
            label3.Text = "Status:";
            // 
            // cbStatus
            // 
            cbStatus.Font = new Font("Segoe UI", 13F);
            cbStatus.FormattingEnabled = true;
            cbStatus.Items.AddRange(new object[] { "Active", "Inactive" });
            cbStatus.Location = new Point(22, 332);
            cbStatus.Name = "cbStatus";
            cbStatus.Size = new Size(334, 44);
            cbStatus.TabIndex = 22;
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Right;
            btnBrowse.BackColor = Color.Blue;
            btnBrowse.Font = new Font("Segoe UI", 10F);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.Location = new Point(414, 316);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(173, 60);
            btnBrowse.TabIndex = 21;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = false;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtStaff
            // 
            txtStaff.Font = new Font("Segoe UI", 13F);
            txtStaff.Location = new Point(22, 156);
            txtStaff.Name = "txtStaff";
            txtStaff.Size = new Size(334, 42);
            txtStaff.TabIndex = 20;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 11F);
            label2.Location = new Point(22, 123);
            label2.Name = "label2";
            label2.Size = new Size(76, 30);
            label2.TabIndex = 19;
            label2.Text = "Name:";
            // 
            // panel2
            // 
            panel2.BackColor = Color.Blue;
            panel2.Controls.Add(btnClose);
            panel2.Controls.Add(btnSave);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 407);
            panel2.Name = "panel2";
            panel2.Size = new Size(631, 87);
            panel2.TabIndex = 17;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.BorderRadius = 10;
            btnClose.BorderThickness = 1;
            btnClose.CustomizableEdges = customizableEdges1;
            btnClose.DisabledState.BorderColor = Color.DarkGray;
            btnClose.DisabledState.CustomBorderColor = Color.DarkGray;
            btnClose.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnClose.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnClose.FillColor = Color.FromArgb(224, 224, 224);
            btnClose.Font = new Font("Segoe UI", 11F);
            btnClose.ForeColor = Color.Black;
            btnClose.Location = new Point(462, 14);
            btnClose.Name = "btnClose";
            btnClose.ShadowDecoration.CustomizableEdges = customizableEdges2;
            btnClose.Size = new Size(140, 60);
            btnClose.TabIndex = 3;
            btnClose.Text = "Close";
            btnClose.Click += btnClose_Click;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnSave.BorderRadius = 10;
            btnSave.CustomizableEdges = customizableEdges3;
            btnSave.DisabledState.BorderColor = Color.DarkGray;
            btnSave.DisabledState.CustomBorderColor = Color.DarkGray;
            btnSave.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnSave.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnSave.FillColor = Color.LimeGreen;
            btnSave.Font = new Font("Segoe UI", 11F);
            btnSave.ForeColor = Color.White;
            btnSave.HoverState.FillColor = Color.FromArgb(0, 192, 0);
            btnSave.Location = new Point(302, 14);
            btnSave.Name = "btnSave";
            btnSave.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnSave.Size = new Size(140, 60);
            btnSave.TabIndex = 2;
            btnSave.Text = "Save";
            btnSave.Click += btnSave_Click;
            // 
            // panel1
            // 
            panel1.BackColor = Color.Black;
            panel1.Controls.Add(guna2Button1);
            panel1.Controls.Add(lblProduct);
            panel1.Dock = DockStyle.Top;
            panel1.ForeColor = Color.White;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(631, 99);
            panel1.TabIndex = 16;
            // 
            // guna2Button1
            // 
            guna2Button1.BorderColor = Color.White;
            guna2Button1.BorderRadius = 10;
            guna2Button1.BorderThickness = 2;
            guna2Button1.CustomizableEdges = customizableEdges5;
            guna2Button1.DisabledState.BorderColor = Color.DarkGray;
            guna2Button1.DisabledState.CustomBorderColor = Color.DarkGray;
            guna2Button1.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            guna2Button1.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            guna2Button1.FillColor = Color.Black;
            guna2Button1.Font = new Font("Segoe UI", 9F);
            guna2Button1.ForeColor = Color.White;
            guna2Button1.Image = (Image)resources.GetObject("guna2Button1.Image");
            guna2Button1.ImageSize = new Size(60, 60);
            guna2Button1.Location = new Point(15, 14);
            guna2Button1.Margin = new Padding(0);
            guna2Button1.Name = "guna2Button1";
            guna2Button1.ShadowDecoration.CustomizableEdges = customizableEdges6;
            guna2Button1.Size = new Size(75, 70);
            guna2Button1.TabIndex = 4;
            // 
            // lblProduct
            // 
            lblProduct.AutoSize = true;
            lblProduct.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblProduct.Location = new Point(94, 36);
            lblProduct.Name = "lblProduct";
            lblProduct.Size = new Size(112, 32);
            lblProduct.TabIndex = 3;
            lblProduct.Text = "Edit Staff";
            // 
            // cbRole
            // 
            cbRole.Font = new Font("Segoe UI", 13F);
            cbRole.FormattingEnabled = true;
            cbRole.Items.AddRange(new object[] { "Admin", "Employee" });
            cbRole.Location = new Point(22, 244);
            cbRole.Name = "cbRole";
            cbRole.Size = new Size(334, 44);
            cbRole.TabIndex = 26;
            // 
            // pbImage
            // 
            pbImage.Image = Properties.Resources.photo1;
            pbImage.Location = new Point(414, 123);
            pbImage.Name = "pbImage";
            pbImage.Size = new Size(173, 165);
            pbImage.SizeMode = PictureBoxSizeMode.StretchImage;
            pbImage.TabIndex = 27;
            pbImage.TabStop = false;
            // 
            // FormEditStaff
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(631, 494);
            Controls.Add(pbImage);
            Controls.Add(cbRole);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(cbStatus);
            Controls.Add(btnBrowse);
            Controls.Add(txtStaff);
            Controls.Add(label2);
            Controls.Add(panel2);
            Controls.Add(panel1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormEditStaff";
            Text = "FormAddStaff";
            panel2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbImage).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (Pen pen = new Pen(Color.Black, 2))
            {
                Rectangle rect = this.ClientRectangle;
                rect.Width -= 1;
                rect.Height -= 1;

                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        #endregion
        private Label label4;
        private Label label3;
        private ComboBox cbStatus;
        private Button btnBrowse;
        private TextBox txtStaff;
        private Label label2;
        public Panel panel2;
        private Guna.UI2.WinForms.Guna2Button btnClose;
        private Guna.UI2.WinForms.Guna2Button btnSave;
        public Panel panel1;
        private Guna.UI2.WinForms.Guna2Button guna2Button1;
        private Label lblProduct;
        private ComboBox cbRole;
        private PictureBox pbImage;
    }
}