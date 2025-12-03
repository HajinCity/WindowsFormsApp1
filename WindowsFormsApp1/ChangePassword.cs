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
    public partial class ChangePassword : Form
    {
        private int loggedInUserId;

        public ChangePassword(int userId)
        {
            InitializeComponent();
            loggedInUserId = userId;
            changebtn.Click += Changebtn_Click;
            
            // Set password fields to use password characters
            CurrentPassword.UseSystemPasswordChar = true;
            NewPassword.UseSystemPasswordChar = true;
            ConfirmNewPassword.UseSystemPasswordChar = true;
        }

        private void Changebtn_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                ChangeUserPassword();
            }
        }

        private bool ValidateInputs()
        {
            // Check if all fields are filled
            if (string.IsNullOrWhiteSpace(CurrentPassword?.Text))
            {
                MessageBox.Show("Please enter your current password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                CurrentPassword?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewPassword?.Text))
            {
                MessageBox.Show("Please enter a new password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NewPassword?.Focus();
                return false;
            }

            // Validate new password length (minimum 8 characters)
            if (NewPassword.Text.Length < 8)
            {
                MessageBox.Show("New password must be at least 8 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NewPassword?.Focus();
                NewPassword.SelectAll();
                return false;
            }

            if (string.IsNullOrWhiteSpace(ConfirmNewPassword?.Text))
            {
                MessageBox.Show("Please confirm your new password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ConfirmNewPassword?.Focus();
                return false;
            }

            // Validate password match
            if (NewPassword.Text != ConfirmNewPassword.Text)
            {
                MessageBox.Show("New password and confirm password do not match. Please try again.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NewPassword?.Focus();
                NewPassword.SelectAll();
                return false;
            }

            // Check if new password is different from current password
            if (CurrentPassword.Text == NewPassword.Text)
            {
                MessageBox.Show("New password must be different from your current password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                NewPassword?.Focus();
                NewPassword.SelectAll();
                return false;
            }

            return true;
        }

        private void ChangeUserPassword()
        {
            try
            {
                string currentPassword = CurrentPassword.Text;
                string newPassword = NewPassword.Text;

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    // First, verify the current password
                    string verifyQuery = "SELECT password_hash FROM users WHERE user_id = @user_id";
                    string storedPasswordHash = null;

                    using (MySqlCommand verifyCommand = new MySqlCommand(verifyQuery, connection))
                    {
                        verifyCommand.Parameters.AddWithValue("@user_id", loggedInUserId);
                        object result = verifyCommand.ExecuteScalar();
                        
                        if (result != null)
                        {
                            storedPasswordHash = result.ToString();
                        }
                        else
                        {
                            MessageBox.Show(
                                "User not found.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Hash the entered current password
                    string enteredCurrentPasswordHash = HashPassword(currentPassword);

                    // Compare hashed passwords (case-insensitive for backward compatibility)
                    bool currentPasswordMatches = enteredCurrentPasswordHash.Equals(storedPasswordHash, StringComparison.OrdinalIgnoreCase);

                    // Backward compatibility: Also check plain text for old accounts
                    if (!currentPasswordMatches)
                    {
                        currentPasswordMatches = currentPassword.Equals(storedPasswordHash);
                    }

                    if (!currentPasswordMatches)
                    {
                        MessageBox.Show(
                            "Current password is incorrect. Please try again.",
                            "Invalid Password",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        CurrentPassword.Focus();
                        CurrentPassword.SelectAll();
                        return;
                    }

                    // Hash the new password
                    string newPasswordHash = HashPassword(newPassword);

                    // Update password in database
                    string updateQuery = @"
                        UPDATE users 
                        SET password_hash = @password_hash
                        WHERE user_id = @user_id";

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@password_hash", newPasswordHash);
                        updateCommand.Parameters.AddWithValue("@user_id", loggedInUserId);

                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Log the activity
                            LogUserActivity(
                                loggedInUserId,
                                "Updated",
                                "Change Password",
                                "User changed their password");

                            MessageBox.Show(
                                "Password changed successfully!",
                                "Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            // Close the form after successful password change
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show(
                                "Failed to update password. Please try again.",
                                "Update Failed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error changing password: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                // Log error but don't block password change
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

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
