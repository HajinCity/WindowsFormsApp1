using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace WindowsFormsApp1
{
    public partial class AddNewUser : Form
    {
        private int loggedInUserId;

        public AddNewUser(int adminUserId)
        {
            InitializeComponent();
            loggedInUserId = adminUserId;
            InitializeComboBoxes();
            CreateBtn.Click += CreateBtn_Click;
            
            // Wire up password visibility toggle events
            pictureBox1.Click += PictureBox1_Click;
            pictureBox3.Click += PictureBox3_Click;
            
            // Initialize pictureBox images and SizeMode
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void InitializeComboBoxes()
        {
            // Initialize Role ComboBox with common roles
            if (Role != null)
            {
                Role.DropDownStyle = ComboBoxStyle.DropDown; // Allow typing
            }

            // Initialize Status ComboBox
            if (Status != null)
            {
                Status.DropDownStyle = ComboBoxStyle.DropDown; // Allow typing
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ParseRangeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CreateBtn_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                CreateUser();
            }
        }

        private bool ValidateInputs()
        {
            // Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(empNo?.Text)) // empNo
            {
                MessageBox.Show("Please enter Employee Number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                empNo?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(fullName?.Text)) // fullName
            {
                MessageBox.Show("Please enter Full Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                fullName?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Position?.Text)) // Position
            {
                MessageBox.Show("Please enter Position.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Position?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Office?.Text)) // Office
            {
                MessageBox.Show("Please enter Office.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Office?.Focus();
                return false;
            }

            if (Role == null || string.IsNullOrWhiteSpace(Role.Text))
            {
                MessageBox.Show("Please select or enter a Role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Role?.Focus();
                return false;
            }

            if (Status == null || string.IsNullOrWhiteSpace(Status.Text))
            {
                MessageBox.Show("Please select or enter a Status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Status?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(username?.Text)) // username
            {
                MessageBox.Show("Please enter Username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                username?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password?.Text)) // password
            {
                MessageBox.Show("Please enter Password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Password?.Focus();
                return false;
            }

            // Validate password length (minimum 8 characters)
            if (Password.Text.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Password?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword?.Text)) // confirmPassword
            {
                MessageBox.Show("Please confirm Password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                confirmPassword?.Focus();
                return false;
            }

            // Validate password match
            if (Password.Text != confirmPassword.Text)
            {
                MessageBox.Show("Password and Confirm Password do not match. Please try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Password?.Focus();
                Password.SelectAll();
                return false;
            }

            return true;
        }

        private void CreateUser()
        {
            try
            {
                string empNoValue = empNo.Text.Trim();
                string fullNameValue = fullName.Text.Trim();
                string position = Position.Text.Trim();
                string office = Office.Text.Trim();
                string role = Role.Text.Trim();
                string status = Status.Text.Trim();
                string usernameValue = username.Text.Trim();
                string password = Password.Text;

                // Hash password using SHA-256
                string passwordHash = HashPassword(password);

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    // Check if username already exists
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                    using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@username", usernameValue);
                        int count = Convert.ToInt32(checkCommand.ExecuteScalar());
                        
                        if (count > 0)
                        {
                            MessageBox.Show(
                                "Username already exists. Please choose a different username.",
                                "Duplicate Username",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            username.Focus();
                            username.SelectAll();
                            return;
                        }
                    }

                    // Insert new user
                    string insertQuery = @"
                        INSERT INTO users 
                            (employee_no, full_name, position, office, username, password_hash, role, status)
                        VALUES 
                            (@employee_no, @full_name, @position, @office, @username, @password_hash, @role, @status)";

                    using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@employee_no", empNoValue);
                        insertCommand.Parameters.AddWithValue("@full_name", fullNameValue);
                        insertCommand.Parameters.AddWithValue("@position", position);
                        insertCommand.Parameters.AddWithValue("@office", office);
                        insertCommand.Parameters.AddWithValue("@username", usernameValue);
                        insertCommand.Parameters.AddWithValue("@password_hash", passwordHash);
                        insertCommand.Parameters.AddWithValue("@role", role);
                        insertCommand.Parameters.AddWithValue("@status", status);

                        insertCommand.ExecuteNonQuery();
                    }

                    // Log the activity
                    LogUserActivity(
                        loggedInUserId,
                        "Created",
                        "User Management",
                        $"Created new user account for {fullNameValue}");

                    MessageBox.Show(
                        $"User account for {fullNameValue} has been created successfully!",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // Clear form fields
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error creating user account: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

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

        private void LogUserActivity(int userId, string action, string module, string details)
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
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
                // Log error but don't block user creation
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

        private void ClearForm()
        {
            empNo.Text = string.Empty;
            fullName.Text = string.Empty;
            Position.Text = string.Empty;
            Office.Text = string.Empty;
            Role.Text = string.Empty;
            Status.Text = string.Empty;
            username.Text = string.Empty;
            Password.Text = string.Empty;
            confirmPassword.Text = string.Empty;
            empNo.Focus();
        }

        // ============================================================
        //  PASSWORD VISIBILITY TOGGLE
        // ============================================================

        /// <summary>
        /// Toggles password visibility for the Password field (pictureBox1)
        /// </summary>
        private void PictureBox1_Click(object sender, EventArgs e)
        {
            if (Password.UseSystemPasswordChar)
            {
                // Password is currently hidden - show it
                Password.UseSystemPasswordChar = false;
                pictureBox1.Image = Properties.Resources.BigEye;
            }
            else
            {
                // Password is currently visible - hide it
                Password.UseSystemPasswordChar = true;
                pictureBox1.Image = Properties.Resources.Closed_Eye;
            }
        }

        /// <summary>
        /// Toggles password visibility for the Confirm Password field (pictureBox3)
        /// </summary>
        private void PictureBox3_Click(object sender, EventArgs e)
        {
            if (confirmPassword.UseSystemPasswordChar)
            {
                // Confirm Password is currently hidden - show it
                confirmPassword.UseSystemPasswordChar = false;
                pictureBox3.Image = Properties.Resources.BigEye;
            }
            else
            {
                // Confirm Password is currently visible - hide it
                confirmPassword.UseSystemPasswordChar = true;
                pictureBox3.Image = Properties.Resources.Closed_Eye;
            }
        }
    }
}
