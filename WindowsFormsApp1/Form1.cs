using System;
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

            // Allow the form to capture key presses globally
            this.KeyPreview = true;
            this.KeyDown += Form_KeyDown;

            // Also attach Enter key for both textboxes
            textBox1.KeyDown += TextBox_KeyDown;
            textBox2.KeyDown += TextBox_KeyDown;
        }

        // ============================================================
        //  GLOBAL ENTER KEY HANDLER
        // ============================================================
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformLogin();
            }
        }

        // ============================================================
        //  TEXTBOX ENTER KEY HANDLER
        // ============================================================
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                PerformLogin();
            }
        }

        // ============================================================
        //  SIGN IN BUTTON CLICK
        // ============================================================
        private void customRoundedButton1_Click(object sender, EventArgs e)
        {
            PerformLogin();
        }

        // ============================================================
        //  LOGIN PROCESS
        // ============================================================
        private void PerformLogin()
        {
            string enteredUsername = textBox1.Text.Trim();
            string enteredPassword = textBox2.Text;

            if (ValidateCredentials(enteredUsername, enteredPassword))
            {
                OpenDashboard();
            }
            else
            {
                MessageBox.Show(
                    "Invalid username or password.",
                    "Login Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        private bool ValidateCredentials(string username, string password)
        {
            return username.Equals(VALID_USERNAME, StringComparison.OrdinalIgnoreCase) &&
                   password.Equals(VALID_PASSWORD);
        }

        // ============================================================
        //  OPEN DASHBOARD
        // ============================================================
        private void OpenDashboard()
        {
            Dashboard dashboard = new Dashboard();
            dashboard.WindowState = FormWindowState.Maximized;
            dashboard.Show();

            this.Hide();
        }

        // Close icon (pictureBox)
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
