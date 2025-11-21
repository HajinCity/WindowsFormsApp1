namespace WindowsFormsApp1
{
    partial class Form1
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
            this.customRoundedPanel1 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.customRoundedButton1 = new WindowsFormsApp1.Controls.CustomRoundedButton();
            this.label4 = new System.Windows.Forms.Label();
            this.customRoundedPanel3 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.customRoundedPanel2 = new WindowsFormsApp1.Controls.CustomRoundedPanel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.gradientPanel1.SuspendLayout();
            this.customRoundedPanel1.SuspendLayout();
            this.customRoundedPanel3.SuspendLayout();
            this.customRoundedPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // gradientPanel1
            // 
            this.gradientPanel1.BackColor = System.Drawing.Color.Transparent;
            this.gradientPanel1.Controls.Add(this.customRoundedPanel1);
            this.gradientPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gradientPanel1.GradientColor1 = System.Drawing.Color.MediumSeaGreen;
            this.gradientPanel1.GradientColor2 = System.Drawing.Color.Turquoise;
            this.gradientPanel1.GradientDirection = WindowsFormsApp1.Controls.GradientDirection.Diagonal;
            this.gradientPanel1.Location = new System.Drawing.Point(0, 0);
            this.gradientPanel1.Name = "gradientPanel1";
            this.gradientPanel1.Size = new System.Drawing.Size(716, 629);
            this.gradientPanel1.TabIndex = 0;
            // 
            // customRoundedPanel1
            // 
            this.customRoundedPanel1.BackColor = System.Drawing.Color.White;
            this.customRoundedPanel1.BorderRadius = 20;
            this.customRoundedPanel1.Controls.Add(this.label5);
            this.customRoundedPanel1.Controls.Add(this.customRoundedButton1);
            this.customRoundedPanel1.Controls.Add(this.label4);
            this.customRoundedPanel1.Controls.Add(this.customRoundedPanel3);
            this.customRoundedPanel1.Controls.Add(this.customRoundedPanel2);
            this.customRoundedPanel1.Controls.Add(this.label3);
            this.customRoundedPanel1.Controls.Add(this.label2);
            this.customRoundedPanel1.Controls.Add(this.label1);
            this.customRoundedPanel1.Location = new System.Drawing.Point(128, 42);
            this.customRoundedPanel1.Name = "customRoundedPanel1";
            this.customRoundedPanel1.Size = new System.Drawing.Size(453, 530);
            this.customRoundedPanel1.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Silver;
            this.label5.Location = new System.Drawing.Point(93, 462);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(259, 22);
            this.label5.TabIndex = 7;
            this.label5.Text = "For authorized personnel only";
            // 
            // customRoundedButton1
            // 
            this.customRoundedButton1.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.customRoundedButton1.BorderRadius = 10;
            this.customRoundedButton1.ClickedColor = System.Drawing.Color.Green;
            this.customRoundedButton1.FlatAppearance.BorderSize = 0;
            this.customRoundedButton1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.customRoundedButton1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.customRoundedButton1.ForeColor = System.Drawing.Color.White;
            this.customRoundedButton1.HoverColor = System.Drawing.Color.SeaGreen;
            this.customRoundedButton1.Location = new System.Drawing.Point(94, 381);
            this.customRoundedButton1.Name = "customRoundedButton1";
            this.customRoundedButton1.Size = new System.Drawing.Size(255, 52);
            this.customRoundedButton1.TabIndex = 6;
            this.customRoundedButton1.Text = "Sign In";
            this.customRoundedButton1.UseVisualStyleBackColor = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(36, 280);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 22);
            this.label4.TabIndex = 5;
            this.label4.Text = "Password";
            // 
            // customRoundedPanel3
            // 
            this.customRoundedPanel3.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel3.BorderRadius = 10;
            this.customRoundedPanel3.Controls.Add(this.textBox2);
            this.customRoundedPanel3.Location = new System.Drawing.Point(59, 305);
            this.customRoundedPanel3.Name = "customRoundedPanel3";
            this.customRoundedPanel3.Size = new System.Drawing.Size(338, 52);
            this.customRoundedPanel3.TabIndex = 4;
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.Color.Gainsboro;
            this.textBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox2.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(14, 18);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(306, 18);
            this.textBox2.TabIndex = 1;
            this.textBox2.UseSystemPasswordChar = true;
            // 
            // customRoundedPanel2
            // 
            this.customRoundedPanel2.BackColor = System.Drawing.Color.Gainsboro;
            this.customRoundedPanel2.BorderRadius = 10;
            this.customRoundedPanel2.Controls.Add(this.textBox1);
            this.customRoundedPanel2.Location = new System.Drawing.Point(59, 181);
            this.customRoundedPanel2.Name = "customRoundedPanel2";
            this.customRoundedPanel2.Size = new System.Drawing.Size(338, 52);
            this.customRoundedPanel2.TabIndex = 3;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.Gainsboro;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(14, 18);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(306, 18);
            this.textBox1.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(36, 156);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 22);
            this.label3.TabIndex = 2;
            this.label3.Text = "Username";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Silver;
            this.label2.Location = new System.Drawing.Point(85, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(274, 22);
            this.label2.TabIndex = 1;
            this.label2.Text = "Financial Management Platform";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.MediumSpringGreen;
            this.label1.Location = new System.Drawing.Point(109, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(231, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Accounts Payable System";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGreen;
            this.ClientSize = new System.Drawing.Size(716, 629);
            this.Controls.Add(this.gradientPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.gradientPanel1.ResumeLayout(false);
            this.customRoundedPanel1.ResumeLayout(false);
            this.customRoundedPanel1.PerformLayout();
            this.customRoundedPanel3.ResumeLayout(false);
            this.customRoundedPanel3.PerformLayout();
            this.customRoundedPanel2.ResumeLayout(false);
            this.customRoundedPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.GradientPanel gradientPanel1;
        private Controls.CustomRoundedPanel customRoundedPanel1;
        private System.Windows.Forms.Label label4;
        private Controls.CustomRoundedPanel customRoundedPanel3;
        private Controls.CustomRoundedPanel customRoundedPanel2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private Controls.CustomRoundedButton customRoundedButton1;
    }
}

