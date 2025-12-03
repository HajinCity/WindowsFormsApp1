namespace WindowsFormsApp1
{
    partial class UpdateUserProfile
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
            this.gradientPanel1 = new WindowsFormsApp1.Controls.GradientPanel();
            this.UpdatePrfBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.CancelBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label9 = new System.Windows.Forms.Label();
            this.customRoundedPanel5 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.UpRole = new System.Windows.Forms.ComboBox();
            this.customRoundedPanel4 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.UpOffice = new System.Windows.Forms.TextBox();
            this.customRoundedPanel3 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.UpPosition = new System.Windows.Forms.TextBox();
            this.customRoundedPanel1 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.UpfullName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.customRoundedPanel2 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.empNo = new System.Windows.Forms.TextBox();
            this.customRoundedPanel6 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.gradientPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.customRoundedPanel5.SuspendLayout();
            this.customRoundedPanel4.SuspendLayout();
            this.customRoundedPanel3.SuspendLayout();
            this.customRoundedPanel1.SuspendLayout();
            this.customRoundedPanel2.SuspendLayout();
            this.customRoundedPanel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gradientPanel1.Controls.Add(this.customRoundedPanel6);
            this.gradientPanel1.Controls.Add(this.label6);
            this.gradientPanel1.Controls.Add(this.UpdatePrfBtn);
            this.gradientPanel1.Controls.Add(this.CancelBtn);
            this.gradientPanel1.Controls.Add(this.pictureBox2);
            this.gradientPanel1.Controls.Add(this.label9);
            this.gradientPanel1.Controls.Add(this.customRoundedPanel5);
            this.gradientPanel1.Controls.Add(this.customRoundedPanel4);
            this.gradientPanel1.Controls.Add(this.customRoundedPanel3);
            this.gradientPanel1.Controls.Add(this.customRoundedPanel1);
            this.gradientPanel1.Controls.Add(this.label5);
            this.gradientPanel1.Controls.Add(this.label4);
            this.gradientPanel1.Controls.Add(this.label3);
            this.gradientPanel1.Controls.Add(this.label1);
            this.gradientPanel1.Controls.Add(this.label2);
            this.gradientPanel1.Controls.Add(this.customRoundedPanel2);
            this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanel1.GradientColor1 = System.Drawing.Color.MediumSeaGreen;
            this.gradientPanel1.GradientColor2 = System.Drawing.Color.Turquoise;
            this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Size = new System.Drawing.Size(539, 510);
            this.gradientPanel1.TabIndex = 1;
            // 
            // UpdatePrfBtn
            // 
            this.UpdatePrfBtn.BackColor = System.Drawing.Color.SeaGreen;
            this.UpdatePrfBtn.BorderRadius = 10;
            this.UpdatePrfBtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.UpdatePrfBtn.FlatAppearance.BorderSize = 0;
            this.UpdatePrfBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UpdatePrfBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpdatePrfBtn.ForeColor = System.Drawing.Color.White;
            this.UpdatePrfBtn.HoverColor = System.Drawing.Color.MediumSeaGreen;
            this.UpdatePrfBtn.Location = new System.Drawing.Point(250, 442);
            this.UpdatePrfBtn.Name = "UpdatePrfBtn";
            this.UpdatePrfBtn.Size = new System.Drawing.Size(190, 43);
            this.UpdatePrfBtn.TabIndex = 151;
            this.UpdatePrfBtn.Text = "Update Profile";
            this.UpdatePrfBtn.UseVisualStyleBackColor = false;
            // 
            // CancelBtn
            // 
            this.CancelBtn.BackColor = System.Drawing.Color.Maroon;
            this.CancelBtn.BorderRadius = 10;
            this.CancelBtn.ClickedColor = System.Drawing.Color.Red;
            this.CancelBtn.FlatAppearance.BorderSize = 0;
            this.CancelBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CancelBtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CancelBtn.ForeColor = System.Drawing.Color.White;
            this.CancelBtn.HoverColor = System.Drawing.Color.DarkRed;
            this.CancelBtn.Location = new System.Drawing.Point(101, 442);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(132, 43);
            this.CancelBtn.TabIndex = 150;
            this.CancelBtn.Text = "Cancel";
            this.CancelBtn.UseVisualStyleBackColor = false;
            this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::WindowsFormsApp1.Properties.Resources.Close;
            this.pictureBox2.Location = new System.Drawing.Point(504, 4);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(30, 30);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 149;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.ForeColor = System.Drawing.Color.Black;
            this.label9.Location = new System.Drawing.Point(203, 15);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(131, 19);
            this.label9.TabIndex = 30;
            this.label9.Text = "Edit Your Profile";
            // 
            // customRoundedPanel5
            // 
            this.customRoundedPanel5.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel5.BorderRadius = 10;
            this.customRoundedPanel5.Controls.Add(this.UpRole);
            this.customRoundedPanel5.Location = new System.Drawing.Point(55, 313);
            this.customRoundedPanel5.Name = "customRoundedPanel5";
            this.customRoundedPanel5.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel5.TabIndex = 8;
            // 
            // UpRole
            // 
            this.UpRole.BackColor = System.Drawing.Color.Gainsboro;
            this.UpRole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.UpRole.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpRole.FormattingEnabled = true;
            this.UpRole.Location = new System.Drawing.Point(14, 7);
            this.UpRole.Name = "UpRole";
            this.UpRole.Size = new System.Drawing.Size(400, 22);
            this.UpRole.TabIndex = 0;
            // 
            // customRoundedPanel4
            // 
            this.customRoundedPanel4.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel4.BorderRadius = 10;
            this.customRoundedPanel4.Controls.Add(this.UpOffice);
            this.customRoundedPanel4.Location = new System.Drawing.Point(55, 253);
            this.customRoundedPanel4.Name = "customRoundedPanel4";
            this.customRoundedPanel4.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel4.TabIndex = 7;
            // 
            // UpOffice
            // 
            this.UpOffice.BackColor = System.Drawing.Color.Gainsboro;
            this.UpOffice.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UpOffice.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpOffice.Location = new System.Drawing.Point(14, 10);
            this.UpOffice.Name = "UpOffice";
            this.UpOffice.Size = new System.Drawing.Size(400, 18);
            this.UpOffice.TabIndex = 0;
            // 
            // customRoundedPanel3
            // 
            this.customRoundedPanel3.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel3.BorderRadius = 10;
            this.customRoundedPanel3.Controls.Add(this.UpPosition);
            this.customRoundedPanel3.Location = new System.Drawing.Point(55, 192);
            this.customRoundedPanel3.Name = "customRoundedPanel3";
            this.customRoundedPanel3.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel3.TabIndex = 6;
            // 
            // UpPosition
            // 
            this.UpPosition.BackColor = System.Drawing.Color.Gainsboro;
            this.UpPosition.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UpPosition.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpPosition.Location = new System.Drawing.Point(14, 10);
            this.UpPosition.Name = "UpPosition";
            this.UpPosition.Size = new System.Drawing.Size(400, 18);
            this.UpPosition.TabIndex = 0;
            // 
            // customRoundedPanel1
            // 
            this.customRoundedPanel1.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel1.BorderRadius = 10;
            this.customRoundedPanel1.Controls.Add(this.UpfullName);
            this.customRoundedPanel1.Location = new System.Drawing.Point(55, 132);
            this.customRoundedPanel1.Name = "customRoundedPanel1";
            this.customRoundedPanel1.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel1.TabIndex = 5;
            // 
            // UpfullName
            // 
            this.UpfullName.BackColor = System.Drawing.Color.Gainsboro;
            this.UpfullName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UpfullName.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UpfullName.Location = new System.Drawing.Point(14, 10);
            this.UpfullName.Name = "UpfullName";
            this.UpfullName.Size = new System.Drawing.Size(400, 18);
            this.UpfullName.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(50, 292);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 18);
            this.label5.TabIndex = 23;
            this.label5.Text = "Role";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(52, 232);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 18);
            this.label4.TabIndex = 21;
            this.label4.Text = "Office";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(52, 171);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 18);
            this.label3.TabIndex = 19;
            this.label3.Text = "Position";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(52, 111);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 18);
            this.label1.TabIndex = 18;
            this.label1.Text = "Fullname";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(52, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 18);
            this.label2.TabIndex = 17;
            this.label2.Text = "Employee No.";
            // 
            // customRoundedPanel2
            // 
            this.customRoundedPanel2.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel2.BorderRadius = 10;
            this.customRoundedPanel2.Controls.Add(this.empNo);
            this.customRoundedPanel2.Location = new System.Drawing.Point(55, 72);
            this.customRoundedPanel2.Name = "customRoundedPanel2";
            this.customRoundedPanel2.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel2.TabIndex = 4;
            // 
            // empNo
            // 
            this.empNo.BackColor = System.Drawing.Color.Gainsboro;
            this.empNo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.empNo.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.empNo.Location = new System.Drawing.Point(14, 10);
            this.empNo.Name = "empNo";
            this.empNo.Size = new System.Drawing.Size(400, 18);
            this.empNo.TabIndex = 0;
            // 
            // customRoundedPanel6
            // 
            this.customRoundedPanel6.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel6.BorderRadius = 10;
            this.customRoundedPanel6.Controls.Add(this.comboBox1);
            this.customRoundedPanel6.Location = new System.Drawing.Point(55, 374);
            this.customRoundedPanel6.Name = "customRoundedPanel6";
            this.customRoundedPanel6.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel6.TabIndex = 152;
            // 
            // comboBox1
            // 
            this.comboBox1.BackColor = System.Drawing.Color.Gainsboro;
            this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Active",
            "Inactive"});
            this.comboBox1.Location = new System.Drawing.Point(14, 7);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(400, 22);
            this.comboBox1.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(50, 353);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(52, 18);
            this.label6.TabIndex = 153;
            this.label6.Text = "Status";
            // 
            // UpdateUserProfile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 510);
            this.Controls.Add(this.gradientPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "UpdateUserProfile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "UpdateUserProfile";
            this.gradientPanel1.ResumeLayout(false);
            this.gradientPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.customRoundedPanel5.ResumeLayout(false);
            this.customRoundedPanel4.ResumeLayout(false);
            this.customRoundedPanel4.PerformLayout();
            this.customRoundedPanel3.ResumeLayout(false);
            this.customRoundedPanel3.PerformLayout();
            this.customRoundedPanel1.ResumeLayout(false);
            this.customRoundedPanel1.PerformLayout();
            this.customRoundedPanel2.ResumeLayout(false);
            this.customRoundedPanel2.PerformLayout();
            this.customRoundedPanel6.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.CustomRoundedButton UpdatePrfBtn;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox UpRole;
        private Controls.CustomRoundedPanel customRoundedPanel5;
        private System.Windows.Forms.TextBox UpOffice;
        private Controls.CustomRoundedButton CancelBtn;
        private Controls.CustomRoundedPanel customRoundedPanel4;
        private Controls.CustomRoundedPanel customRoundedPanel3;
        private System.Windows.Forms.TextBox UpPosition;
        private System.Windows.Forms.TextBox UpfullName;
        private Controls.CustomRoundedPanel customRoundedPanel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox empNo;
        private Controls.CustomRoundedPanel customRoundedPanel2;
        private Controls.GradientPanel gradientPanel1;
        private Controls.CustomRoundedPanel customRoundedPanel6;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label6;
    }
}