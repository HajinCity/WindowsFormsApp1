using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {

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
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = "SELECT password_hash, status FROM users WHERE username = @username";
                    
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Check if user account is active
                                string status = reader["status"]?.ToString() ?? "";
                                if (status.ToLower() != "active")
                                {
                                    return false;
                                }

                                // Since password_hash is not hashed yet, compare directly
                                string storedPassword = reader["password_hash"]?.ToString() ?? "";
                                return password.Equals(storedPassword);
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Database error: {ex.Message}",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return false;
            }
        }

        // ============================================================
        //  OPEN DASHBOARD
        // ============================================================
        private void OpenDashboard()
        {
            Form2 dashboard = new Form2();
            dashboard.WindowState = FormWindowState.Maximized;
            dashboard.SetAutoLoadDashboard(true);
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
