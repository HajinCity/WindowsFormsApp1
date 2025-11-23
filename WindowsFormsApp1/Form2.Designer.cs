namespace WindowsFormsApp1
{
    partial class Form2
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
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SignOutBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.SupplierBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.ReportsBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.IARBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.DashboardBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.ORSBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.DVBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.GJBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.JEVBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(869, 778);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(278, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(591, 778);
            this.panel3.TabIndex = 10;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.SignOutBtn);
            this.panel1.Controls.Add(this.SupplierBtn);
            this.panel1.Controls.Add(this.ReportsBtn);
            this.panel1.Controls.Add(this.IARBtn);
            this.panel1.Controls.Add(this.DashboardBtn);
            this.panel1.Controls.Add(this.ORSBtn);
            this.panel1.Controls.Add(this.DVBtn);
            this.panel1.Controls.Add(this.GJBtn);
            this.panel1.Controls.Add(this.JEVBtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(278, 778);
            this.panel1.TabIndex = 9;
            // 
            // SignOutBtn
            // 
            this.SignOutBtn.BackColor = System.Drawing.Color.White;
            this.SignOutBtn.BorderRadius = 10;
            this.SignOutBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.SignOutBtn.FlatAppearance.BorderSize = 0;
            this.SignOutBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SignOutBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SignOutBtn.ForeColor = System.Drawing.Color.Black;
            this.SignOutBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.SignOutBtn.Image = global::WindowsFormsApp1.Properties.Resources.Logout;
            this.SignOutBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SignOutBtn.Location = new System.Drawing.Point(22, 707);
            this.SignOutBtn.Name = "SignOutBtn";
            this.SignOutBtn.Size = new System.Drawing.Size(237, 50);
            this.SignOutBtn.TabIndex = 9;
            this.SignOutBtn.Text = "Sign Out";
            this.SignOutBtn.UseVisualStyleBackColor = false;
            // 
            // SupplierBtn
            // 
            this.SupplierBtn.BackColor = System.Drawing.Color.White;
            this.SupplierBtn.BorderRadius = 10;
            this.SupplierBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.SupplierBtn.FlatAppearance.BorderSize = 0;
            this.SupplierBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SupplierBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SupplierBtn.ForeColor = System.Drawing.Color.Black;
            this.SupplierBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.SupplierBtn.Image = global::WindowsFormsApp1.Properties.Resources.Group;
            this.SupplierBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.SupplierBtn.Location = new System.Drawing.Point(22, 186);
            this.SupplierBtn.Name = "SupplierBtn";
            this.SupplierBtn.Size = new System.Drawing.Size(237, 50);
            this.SupplierBtn.TabIndex = 2;
            this.SupplierBtn.Text = "Supplier";
            this.SupplierBtn.UseVisualStyleBackColor = false;
            // 
            // ReportsBtn
            // 
            this.ReportsBtn.BackColor = System.Drawing.Color.White;
            this.ReportsBtn.BorderRadius = 10;
            this.ReportsBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.ReportsBtn.FlatAppearance.BorderSize = 0;
            this.ReportsBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReportsBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReportsBtn.ForeColor = System.Drawing.Color.Black;
            this.ReportsBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.ReportsBtn.Image = global::WindowsFormsApp1.Properties.Resources.Graph_Report;
            this.ReportsBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ReportsBtn.Location = new System.Drawing.Point(22, 522);
            this.ReportsBtn.Name = "ReportsBtn";
            this.ReportsBtn.Size = new System.Drawing.Size(237, 50);
            this.ReportsBtn.TabIndex = 8;
            this.ReportsBtn.Text = "Reports";
            this.ReportsBtn.UseVisualStyleBackColor = false;
            // 
            // IARBtn
            // 
            this.IARBtn.BackColor = System.Drawing.Color.White;
            this.IARBtn.BorderRadius = 10;
            this.IARBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.IARBtn.FlatAppearance.BorderSize = 0;
            this.IARBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.IARBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.IARBtn.ForeColor = System.Drawing.Color.Black;
            this.IARBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.IARBtn.Image = global::WindowsFormsApp1.Properties.Resources.Pass;
            this.IARBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.IARBtn.Location = new System.Drawing.Point(22, 298);
            this.IARBtn.Name = "IARBtn";
            this.IARBtn.Size = new System.Drawing.Size(237, 50);
            this.IARBtn.TabIndex = 4;
            this.IARBtn.Text = "IAR";
            this.IARBtn.UseVisualStyleBackColor = false;
            // 
            // DashboardBtn
            // 
            this.DashboardBtn.BackColor = System.Drawing.Color.White;
            this.DashboardBtn.BorderRadius = 10;
            this.DashboardBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.DashboardBtn.FlatAppearance.BorderSize = 0;
            this.DashboardBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DashboardBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DashboardBtn.ForeColor = System.Drawing.Color.Black;
            this.DashboardBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.DashboardBtn.Image = global::WindowsFormsApp1.Properties.Resources.Dashboard_Layout;
            this.DashboardBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DashboardBtn.Location = new System.Drawing.Point(22, 130);
            this.DashboardBtn.Name = "DashboardBtn";
            this.DashboardBtn.Size = new System.Drawing.Size(237, 50);
            this.DashboardBtn.TabIndex = 1;
            this.DashboardBtn.Text = "Dashboard";
            this.DashboardBtn.UseVisualStyleBackColor = false;
            // 
            // ORSBtn
            // 
            this.ORSBtn.BackColor = System.Drawing.Color.White;
            this.ORSBtn.BorderRadius = 10;
            this.ORSBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.ORSBtn.FlatAppearance.BorderSize = 0;
            this.ORSBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ORSBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ORSBtn.ForeColor = System.Drawing.Color.Black;
            this.ORSBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.ORSBtn.Image = global::WindowsFormsApp1.Properties.Resources.Document1;
            this.ORSBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ORSBtn.Location = new System.Drawing.Point(22, 354);
            this.ORSBtn.Name = "ORSBtn";
            this.ORSBtn.Size = new System.Drawing.Size(237, 50);
            this.ORSBtn.TabIndex = 5;
            this.ORSBtn.Text = "ORS-BURS";
            this.ORSBtn.UseVisualStyleBackColor = false;
            // 
            // DVBtn
            // 
            this.DVBtn.BackColor = System.Drawing.Color.White;
            this.DVBtn.BorderRadius = 10;
            this.DVBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.DVBtn.FlatAppearance.BorderSize = 0;
            this.DVBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.DVBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DVBtn.ForeColor = System.Drawing.Color.Black;
            this.DVBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.DVBtn.Image = global::WindowsFormsApp1.Properties.Resources.Credit_Card;
            this.DVBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.DVBtn.Location = new System.Drawing.Point(22, 466);
            this.DVBtn.Name = "DVBtn";
            this.DVBtn.Size = new System.Drawing.Size(237, 50);
            this.DVBtn.TabIndex = 7;
            this.DVBtn.Text = "Disbursement Voucher";
            this.DVBtn.UseVisualStyleBackColor = false;
            // 
            // GJBtn
            // 
            this.GJBtn.BackColor = System.Drawing.Color.White;
            this.GJBtn.BorderRadius = 10;
            this.GJBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.GJBtn.FlatAppearance.BorderSize = 0;
            this.GJBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GJBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GJBtn.ForeColor = System.Drawing.Color.Black;
            this.GJBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.GJBtn.Image = global::WindowsFormsApp1.Properties.Resources.Document1;
            this.GJBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.GJBtn.Location = new System.Drawing.Point(22, 242);
            this.GJBtn.Name = "GJBtn";
            this.GJBtn.Size = new System.Drawing.Size(237, 50);
            this.GJBtn.TabIndex = 3;
            this.GJBtn.Text = "General Journal";
            this.GJBtn.UseVisualStyleBackColor = false;
            // 
            // JEVBtn
            // 
            this.JEVBtn.BackColor = System.Drawing.Color.White;
            this.JEVBtn.BorderRadius = 10;
            this.JEVBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.JEVBtn.FlatAppearance.BorderSize = 0;
            this.JEVBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.JEVBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.JEVBtn.ForeColor = System.Drawing.Color.Black;
            this.JEVBtn.HoverColor = System.Drawing.Color.Gainsboro;
            this.JEVBtn.Image = global::WindowsFormsApp1.Properties.Resources.Document1;
            this.JEVBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.JEVBtn.Location = new System.Drawing.Point(22, 410);
            this.JEVBtn.Name = "JEVBtn";
            this.JEVBtn.Size = new System.Drawing.Size(237, 50);
            this.JEVBtn.TabIndex = 6;
            this.JEVBtn.Text = "JEV";
            this.JEVBtn.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.label1.Location = new System.Drawing.Point(28, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(231, 22);
            this.label1.TabIndex = 1;
            this.label1.Text = "Accounts Payable System";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 778);
            this.Controls.Add(this.panel2);
            this.Name = "Form2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form2";
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private Controls.CustomRoundedButton ReportsBtn;
        private Controls.CustomRoundedButton DVBtn;
        private Controls.CustomRoundedButton JEVBtn;
        private Controls.CustomRoundedButton ORSBtn;
        private Controls.CustomRoundedButton IARBtn;
        private Controls.CustomRoundedButton GJBtn;
        private Controls.CustomRoundedButton SupplierBtn;
        private Controls.CustomRoundedButton DashboardBtn;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private Controls.CustomRoundedButton SignOutBtn;
        private System.Windows.Forms.Label label1;
    }
}