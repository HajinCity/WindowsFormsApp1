using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Static login credentials
        private const string VALID_USERNAME = "cpa";
        private const string VALID_PASSWORD = "KDAahri";

        public Form1()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void customRoundedButton1_Click(object sender, EventArgs e)
        {
            // Get entered username and password
            string enteredUsername = textBox1.Text.Trim();
            string enteredPassword = textBox2.Text;

            // Validate credentials
            if (ValidateCredentials(enteredUsername, enteredPassword))
            {
                // Credentials are correct - open Dashboard
                OpenDashboard();
            }
            else
            {
                // Credentials are incorrect - show error message
                MessageBox.Show(
                    "Invalid username or password.",
                    "Login Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        /// <summary>
        /// Validates the entered username and password against the static credentials
        /// </summary>
        /// <param name="username">Entered username</param>
        /// <param name="password">Entered password</param>
        /// <returns>True if credentials are valid, false otherwise</returns>
        private bool ValidateCredentials(string username, string password)
        {
            return username.Equals(VALID_USERNAME, StringComparison.OrdinalIgnoreCase) &&
                   password.Equals(VALID_PASSWORD);
        }

        /// <summary>
        /// Closes the login form and opens the Dashboard form maximized
        /// </summary>
        private void OpenDashboard()
        {
            // Create and configure the Dashboard form
            Dashboard dashboard = new Dashboard();
            dashboard.WindowState = FormWindowState.Maximized;

            // Show the Dashboard
            dashboard.Show();

            // Close the login form
            this.Hide();
        }
    }
}
