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

namespace WindowsFormsApp1
{
    public partial class UpdateUserProfile : Form
    {
        private int loggedInUserId;

        public UpdateUserProfile(int userId)
        {
            InitializeComponent();
            loggedInUserId = userId;
            InitializeComboBoxes();
            LoadUserProfile();
            UpdatePrfBtn.Click += UpdatePrfBtn_Click;
        }

        private void InitializeComboBoxes()
        {
            // Initialize Role ComboBox
            if (UpRole != null)
            {
                UpRole.DropDownStyle = ComboBoxStyle.DropDown; // Allow typing
            }

            // Initialize Status ComboBox
            if (comboBox1 != null)
            {
                comboBox1.DropDownStyle = ComboBoxStyle.DropDown; // Allow typing
            }
        }

        private void LoadUserProfile()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"
                        SELECT employee_no, full_name, position, office, role, status
                        FROM users
                        WHERE user_id = @user_id";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", loggedInUserId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate form fields with current user data
                                empNo.Text = reader["employee_no"]?.ToString() ?? "";
                                UpfullName.Text = reader["full_name"]?.ToString() ?? "";
                                UpPosition.Text = reader["position"]?.ToString() ?? "";
                                UpOffice.Text = reader["office"]?.ToString() ?? "";
                                UpRole.Text = reader["role"]?.ToString() ?? "";
                                comboBox1.Text = reader["status"]?.ToString() ?? "";
                            }
                            else
                            {
                                MessageBox.Show(
                                    "User profile not found.",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading user profile: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdatePrfBtn_Click(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                UpdateProfile();
            }
        }

        private bool ValidateInputs()
        {
            // Check if all required fields are filled
            if (string.IsNullOrWhiteSpace(empNo?.Text))
            {
                MessageBox.Show("Please enter Employee Number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                empNo?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(UpfullName?.Text))
            {
                MessageBox.Show("Please enter Full Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpfullName?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(UpPosition?.Text))
            {
                MessageBox.Show("Please enter Position.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpPosition?.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(UpOffice?.Text))
            {
                MessageBox.Show("Please enter Office.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpOffice?.Focus();
                return false;
            }

            if (UpRole == null || string.IsNullOrWhiteSpace(UpRole.Text))
            {
                MessageBox.Show("Please select or enter a Role.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UpRole?.Focus();
                return false;
            }

            if (comboBox1 == null || string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                MessageBox.Show("Please select or enter a Status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1?.Focus();
                return false;
            }

            return true;
        }

        private void UpdateProfile()
        {
            try
            {
                string empNoValue = empNo.Text.Trim();
                string fullNameValue = UpfullName.Text.Trim();
                string position = UpPosition.Text.Trim();
                string office = UpOffice.Text.Trim();
                string role = UpRole.Text.Trim();
                string status = comboBox1.Text.Trim();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    // Update user profile
                    string updateQuery = @"
                        UPDATE users 
                        SET employee_no = @employee_no,
                            full_name = @full_name,
                            position = @position,
                            office = @office,
                            role = @role,
                            status = @status
                        WHERE user_id = @user_id";

                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@employee_no", empNoValue);
                        updateCommand.Parameters.AddWithValue("@full_name", fullNameValue);
                        updateCommand.Parameters.AddWithValue("@position", position);
                        updateCommand.Parameters.AddWithValue("@office", office);
                        updateCommand.Parameters.AddWithValue("@role", role);
                        updateCommand.Parameters.AddWithValue("@status", status);
                        updateCommand.Parameters.AddWithValue("@user_id", loggedInUserId);

                        int rowsAffected = updateCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Log the activity
                            LogUserActivity(
                                loggedInUserId,
                                "Updated",
                                "User Management",
                                $"Updated user profile for {fullNameValue}");

                            MessageBox.Show(
                                "Profile updated successfully!",
                                "Success",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show(
                                "No changes were made to the profile.",
                                "Update Failed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error updating profile: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
                // Log error but don't block profile update
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

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
