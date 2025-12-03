namespace WindowsFormsApp1
{
    partial class ChangePassword
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
            this.changebtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.CancelBtn = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label9 = new System.Windows.Forms.Label();
            this.customRoundedPanel3 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.ConfirmNewPassword = new System.Windows.Forms.TextBox();
            this.customRoundedPanel1 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.NewPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.customRoundedPanel2 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.CurrentPassword = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.gradientPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.customRoundedPanel3.SuspendLayout();
            this.customRoundedPanel1.SuspendLayout();
            this.customRoundedPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            this.SuspendLayout();
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gradientPanel1.Controls.Add(this.changebtn);
            this.gradientPanel1.Controls.Add(this.CancelBtn);
            this.gradientPanel1.Controls.Add(this.pictureBox2);
            this.gradientPanel1.Controls.Add(this.label9);
            this.gradientPanel1.Controls.Add(this.customRoundedPanel3);
            this.gradientPanel1.Controls.Add(this.customRoundedPanel1);
            this.gradientPanel1.Controls.Add(this.label3);
            this.gradientPanel1.Controls.Add(this.label1);
            this.gradientPanel1.Controls.Add(this.label2);
            this.gradientPanel1.Controls.Add(this.customRoundedPanel2);
            this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanel1.GradientColor1 = System.Drawing.Color.MediumSeaGreen;
            this.gradientPanel1.GradientColor2 = System.Drawing.Color.Turquoise;
            this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Size = new System.Drawing.Size(539, 469);
            this.gradientPanel1.TabIndex = 2;
            // 
            // changebtn
            // 
            this.changebtn.BackColor = System.Drawing.Color.SeaGreen;
            this.changebtn.BorderRadius = 10;
            this.changebtn.ClickedColor = System.Drawing.Color.SeaGreen;
            this.changebtn.FlatAppearance.BorderSize = 0;
            this.changebtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.changebtn.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.changebtn.ForeColor = System.Drawing.Color.White;
            this.changebtn.HoverColor = System.Drawing.Color.MediumSeaGreen;
            this.changebtn.Location = new System.Drawing.Point(227, 388);
            this.changebtn.Name = "changebtn";
            this.changebtn.Size = new System.Drawing.Size(219, 43);
            this.changebtn.TabIndex = 151;
            this.changebtn.Text = "Change Password";
            this.changebtn.UseVisualStyleBackColor = false;
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
            this.CancelBtn.Location = new System.Drawing.Point(89, 388);
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
            this.label9.Location = new System.Drawing.Point(188, 15);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(150, 19);
            this.label9.TabIndex = 30;
            this.label9.Text = "Change Password";
            // 
            // customRoundedPanel3
            // 
            this.customRoundedPanel3.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel3.BorderRadius = 10;
            this.customRoundedPanel3.Controls.Add(this.pictureBox4);
            this.customRoundedPanel3.Controls.Add(this.ConfirmNewPassword);
            this.customRoundedPanel3.Location = new System.Drawing.Point(55, 282);
            this.customRoundedPanel3.Name = "customRoundedPanel3";
            this.customRoundedPanel3.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel3.TabIndex = 6;
            // 
            // ConfirmNewPassword
            // 
            this.ConfirmNewPassword.BackColor = System.Drawing.Color.Gainsboro;
            this.ConfirmNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ConfirmNewPassword.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConfirmNewPassword.Location = new System.Drawing.Point(14, 10);
            this.ConfirmNewPassword.Name = "ConfirmNewPassword";
            this.ConfirmNewPassword.Size = new System.Drawing.Size(372, 18);
            this.ConfirmNewPassword.TabIndex = 0;
            // 
            // customRoundedPanel1
            // 
            this.customRoundedPanel1.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel1.BorderRadius = 10;
            this.customRoundedPanel1.Controls.Add(this.pictureBox3);
            this.customRoundedPanel1.Controls.Add(this.NewPassword);
            this.customRoundedPanel1.Location = new System.Drawing.Point(55, 193);
            this.customRoundedPanel1.Name = "customRoundedPanel1";
            this.customRoundedPanel1.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel1.TabIndex = 5;
            // 
            // NewPassword
            // 
            this.NewPassword.BackColor = System.Drawing.Color.Gainsboro;
            this.NewPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.NewPassword.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewPassword.Location = new System.Drawing.Point(14, 10);
            this.NewPassword.Name = "NewPassword";
            this.NewPassword.Size = new System.Drawing.Size(372, 18);
            this.NewPassword.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(52, 261);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(172, 18);
            this.label3.TabIndex = 19;
            this.label3.Text = "Confirm New Password";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(52, 172);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 18);
            this.label1.TabIndex = 18;
            this.label1.Text = "New Password";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(52, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 18);
            this.label2.TabIndex = 17;
            this.label2.Text = "Current Password";
            // 
            // customRoundedPanel2
            // 
            this.customRoundedPanel2.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel2.BorderRadius = 10;
            this.customRoundedPanel2.Controls.Add(this.pictureBox1);
            this.customRoundedPanel2.Controls.Add(this.CurrentPassword);
            this.customRoundedPanel2.Location = new System.Drawing.Point(55, 112);
            this.customRoundedPanel2.Name = "customRoundedPanel2";
            this.customRoundedPanel2.Size = new System.Drawing.Size(426, 36);
            this.customRoundedPanel2.TabIndex = 4;
            // 
            // CurrentPassword
            // 
            this.CurrentPassword.BackColor = System.Drawing.Color.Gainsboro;
            this.CurrentPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CurrentPassword.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CurrentPassword.Location = new System.Drawing.Point(14, 10);
            this.CurrentPassword.Name = "CurrentPassword";
            this.CurrentPassword.Size = new System.Drawing.Size(372, 18);
            this.CurrentPassword.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::WindowsFormsApp1.Properties.Resources.Closed_Eye;
            this.pictureBox1.Location = new System.Drawing.Point(392, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(31, 28);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::WindowsFormsApp1.Properties.Resources.Closed_Eye;
            this.pictureBox3.Location = new System.Drawing.Point(392, 5);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(31, 28);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox3.TabIndex = 3;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::WindowsFormsApp1.Properties.Resources.Closed_Eye;
            this.pictureBox4.Location = new System.Drawing.Point(392, 5);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(31, 28);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox4.TabIndex = 4;
            this.pictureBox4.TabStop = false;
            // 
            // ChangePassword
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(539, 469);
            this.Controls.Add(this.gradientPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ChangePassword";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ChangePassword";
            this.gradientPanel1.ResumeLayout(false);
            this.gradientPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.customRoundedPanel3.ResumeLayout(false);
            this.customRoundedPanel3.PerformLayout();
            this.customRoundedPanel1.ResumeLayout(false);
            this.customRoundedPanel1.PerformLayout();
            this.customRoundedPanel2.ResumeLayout(false);
            this.customRoundedPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox2;
        private Controls.CustomRoundedPanel customRoundedPanel2;
        private System.Windows.Forms.TextBox CurrentPassword;
        private Controls.CustomRoundedPanel customRoundedPanel1;
        private System.Windows.Forms.TextBox NewPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ConfirmNewPassword;
        private Controls.CustomRoundedButton CancelBtn;
        private Controls.CustomRoundedPanel customRoundedPanel3;
        private System.Windows.Forms.Label label9;
        private Controls.GradientPanel gradientPanel1;
        private Controls.CustomRoundedButton changebtn;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox3;
    }
}