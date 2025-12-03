using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private int loggedInUserId = 0;

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
                // Log successful login
                LogUserActivity(
                    loggedInUserId,
                    "Login",
                    "Authentication",
                    "User logged in successfully");

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

        private string loggedInUserFullName = string.Empty;

        private bool ValidateCredentials(string username, string password)
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = "SELECT user_id, password_hash, status, full_name FROM users WHERE username = @username";
                    
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

                                // Get stored password hash from database
                                string storedPasswordHash = reader["password_hash"]?.ToString() ?? "";
                                
                                // Hash the entered password using SHA-256
                                string enteredPasswordHash = HashPassword(password);
                                
                                // Compare hashed passwords (case-sensitive comparison)
                                if (enteredPasswordHash.Equals(storedPasswordHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Retrieve user_id and full_name for the logged-in user
                                    loggedInUserId = reader.GetInt32(reader.GetOrdinal("user_id"));
                                    loggedInUserFullName = reader["full_name"]?.ToString() ?? "";
                                    
                                    MessageBox.Show(
                                        "Welcome user!",
                                        "Login Successful",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information
                                    );
                                    return true;
                                }
                                
                                // Backward compatibility: Check plain text password for old accounts
                                // (This allows existing accounts with plain text passwords to still work)
                                if (password.Equals(storedPasswordHash))
                                {
                                    // Retrieve user_id and full_name for the logged-in user
                                    loggedInUserId = reader.GetInt32(reader.GetOrdinal("user_id"));
                                    loggedInUserFullName = reader["full_name"]?.ToString() ?? "";
                                    
                                    MessageBox.Show(
                                        "Welcome user!",
                                        "Login Successful",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information
                                    );
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
                MessageBox.Show(
                    "We could not complete the sign-in right now. Please try again or contact your system administrator.",
                    "Connection Issue",
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
            dashboard.SetLoggedInUserFullName(loggedInUserFullName);
            dashboard.SetLoggedInUserId(loggedInUserId);
            dashboard.WindowState = FormWindowState.Maximized;
            dashboard.SetAutoLoadDashboard(true);
            dashboard.Show();

            this.Hide();
        }

        public void ResetLoginFields()
        {
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox1.Focus();
        }

        // Close icon (pictureBox)
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // ============================================================
        //  USER LOGGING
        // ============================================================
        private void LogUserActivity(int userId, string action, string module, string details)
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    // Insert log and populate the `users` column with the user's full name
                    string query = @"
                        INSERT INTO userlogs (user_id, users, action, module, details, ip_address, action_timestamp)
                        SELECT 
                            u.user_id,
                            u.full_name,
                            @action,
                            @module,
                            @details,
                            @ip_address,
                            NOW()
                        FROM users u
                        WHERE u.user_id = @user_id;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        command.Parameters.AddWithValue("@action", action);
                        command.Parameters.AddWithValue("@module", module);
                        command.Parameters.AddWithValue("@details", details ?? string.Empty);
                        command.Parameters.AddWithValue("@ip_address", GetLocalIpAddress());

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Do not block login on logging failures, just trace it.
                System.Diagnostics.Debug.WriteLine($"Failed to log user activity: {ex.Message}");
            }
        }

        private string GetLocalIpAddress()
        {
            try
            {
                string localIP = "";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }

                return string.IsNullOrEmpty(localIP) ? "Unknown" : localIP;
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Hashes a password using SHA-256 algorithm
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The SHA-256 hash as a hexadecimal string</returns>
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Compute hash from the password
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert byte array to a hex string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
