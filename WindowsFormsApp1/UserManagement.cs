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
using System.IO;

namespace WindowsFormsApp1
{
    public partial class UserManagement : Form
    {
        private class UserRecord
        {
            public int UserId { get; set; }
            public string FullName { get; set; }
            public string EmployeeNo { get; set; }
            public string Position { get; set; }
            public string Role { get; set; }
            public string Status { get; set; }
        }

        private List<UserRecord> userCache = new List<UserRecord>();
        private DataTable userLogsTable = new DataTable();
        private int loggedInUserId = 0;

        public UserManagement()
        {
            InitializeComponent();
            LoadUsers();
            LoadUserLogs();
            textBox2.TextChanged += TextBox2_TextChanged;
            textBox3.TextChanged += TextBox3_TextChanged;
            EnterBtn.Click += EnterBtn_Click;
            pictureBox5.Click += PictureBox5_Click;
            AddUserBtn.Click += AddUserBtn_Click;
            ChangepassBtn.Click += ChangepassBtn_Click;
            EditProfileBtn.Click += EditProfileBtn_Click;
            exportBtn.Click += ExportBtn_Click;
        }

        public void SetLoggedInUserId(int userId)
        {
            loggedInUserId = userId;
        }

        private void LoadUsers()
        {
            try
            {
                userCache.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT user_id, full_name, employee_no, position, role, status
                                     FROM users
                                     ORDER BY full_name";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userCache.Add(new UserRecord
                            {
                                UserId = reader.GetInt32("user_id"),
                                FullName = reader["full_name"]?.ToString() ?? "",
                                EmployeeNo = reader["employee_no"]?.ToString() ?? "",
                                Position = reader["position"]?.ToString() ?? "",
                                Role = reader["role"]?.ToString() ?? "",
                                Status = reader["status"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                // Display all data initially
                DisplayAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load users: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // Refresh logs and clear filters when pictureBox5 is clicked
        private void PictureBox5_Click(object sender, EventArgs e)
        {
            // Reload from database
            LoadUserLogs();

            // Clear filter controls
            textBox3.Text = string.Empty;
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0; // typically "All Actions"
            }

            // Reset date filter to today but don't force-enable it
            dateTimePicker3.Value = DateTime.Today;

            // Ensure grid shows all rows from the refreshed table
            if (userLogsTable != null)
            {
                userLogsTable.DefaultView.RowFilter = string.Empty;
                dataGridView2.DataSource = userLogsTable;
            }
        }

        private void DisplayAllData()
        {
            dataGridView1.Rows.Clear();
            foreach (var user in userCache)
            {
                int rowIndex = dataGridView1.Rows.Add();
                dataGridView1.Rows[rowIndex].Cells["Column1"].Value = user.FullName;
                dataGridView1.Rows[rowIndex].Cells["Column2"].Value = user.EmployeeNo;
                dataGridView1.Rows[rowIndex].Cells["Column3"].Value = user.Position;
                dataGridView1.Rows[rowIndex].Cells["Column4"].Value = user.Role;
                dataGridView1.Rows[rowIndex].Cells["Column5"].Value = user.Status;
                dataGridView1.Rows[rowIndex].Tag = user.UserId;
            }
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            ApplyUserFilter();
        }

        private void ApplyUserFilter()
        {
            string term = (textBox2.Text ?? string.Empty).Trim().ToLowerInvariant();

            var filtered = userCache.Where(user =>
                string.IsNullOrEmpty(term) ||
                (user.FullName ?? string.Empty).ToLowerInvariant().Contains(term) ||
                (user.EmployeeNo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                (user.Position ?? string.Empty).ToLowerInvariant().Contains(term) ||
                (user.Role ?? string.Empty).ToLowerInvariant().Contains(term) ||
                (user.Status ?? string.Empty).ToLowerInvariant().Contains(term));

            dataGridView1.Rows.Clear();
            foreach (var user in filtered)
            {
                int rowIndex = dataGridView1.Rows.Add();
                dataGridView1.Rows[rowIndex].Cells["Column1"].Value = user.FullName;
                dataGridView1.Rows[rowIndex].Cells["Column2"].Value = user.EmployeeNo;
                dataGridView1.Rows[rowIndex].Cells["Column3"].Value = user.Position;
                dataGridView1.Rows[rowIndex].Cells["Column4"].Value = user.Role;
                dataGridView1.Rows[rowIndex].Cells["Column5"].Value = user.Status;
                dataGridView1.Rows[rowIndex].Tag = user.UserId;
            }
        }

        /// <summary>
        /// Loads all records from userlogs (joined with users)
        /// and binds them to dataGridView2.
        /// </summary>
        private void LoadUserLogs()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"
                        SELECT 
                            ul.action_timestamp   AS log_timestamp,
                            u.full_name           AS user_fullname,
                            ul.action             AS user_action,
                            ul.module             AS module,
                            ul.details            AS details,
                            ul.ip_address         AS ip_address
                        FROM userlogs ul
                        INNER JOIN users u ON ul.user_id = u.user_id
                        ORDER BY ul.action_timestamp DESC";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        userLogsTable = new DataTable();
                        adapter.Fill(userLogsTable);

                        // We are using designer-created columns, so don't auto-generate.
                        dataGridView2.AutoGenerateColumns = false;

                        // Map each DataGridView column to the corresponding DataTable column.
                        // Make sure these column names (dataGridViewTextBoxColumnX) match your designer.
                        dataGridViewTextBoxColumn1.DataPropertyName = "log_timestamp";   // Timestamp
                        dataGridViewTextBoxColumn2.DataPropertyName = "user_fullname";   // User (full name)
                        dataGridViewTextBoxColumn3.DataPropertyName = "user_action";     // Action
                        dataGridViewTextBoxColumn4.DataPropertyName = "module";          // Module
                        dataGridViewTextBoxColumn5.DataPropertyName = "details";         // Details
                        Column6.DataPropertyName = "ip_address";      // IP Address

                        dataGridView2.DataSource = userLogsTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load user logs: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ============================================================
        //  USER LOGS FILTERING (dataGridView2)
        // ============================================================

        // 1) Keyword-only filter (TextChanged on textBox3)
        private void TextBox3_TextChanged(object sender, EventArgs e)
        {
            ApplyUserLogsFilter(
                keyword: textBox3.Text,
                actionFilter: null,
                dateFilter: null);
        }

        // 2 & 3) Keyword + Action (+ optional Date) filter (EnterBtn)
        private void EnterBtn_Click(object sender, EventArgs e)
        {
            string keyword = textBox3.Text;
            string selectedAction = comboBox1.SelectedItem as string;

            // Treat "All Actions" and empty selection as no action filter
            if (string.IsNullOrWhiteSpace(selectedAction) ||
                selectedAction.Equals("All Actions", StringComparison.OrdinalIgnoreCase))
            {
                selectedAction = null;
            }

            // Always use the date value when EnterBtn is clicked and the control is enabled.
            // If you only want the date when a checkbox is checked, you can adjust this.
            DateTime? dateFilter = dateTimePicker3.Enabled
                ? dateTimePicker3.Value.Date
                : (DateTime?)null;

            ApplyUserLogsFilter(
                keyword: keyword,
                actionFilter: selectedAction,
                dateFilter: dateFilter);
        }

        private void ApplyUserLogsFilter(string keyword, string actionFilter, DateTime? dateFilter)
        {
            if (userLogsTable == null)
            {
                return;
            }

            var filters = new List<string>();

            // Keyword filter across user_fullname, user_action, module, details
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string escaped = keyword.Replace("'", "''");
                filters.Add(
                    $"(user_fullname LIKE '%{escaped}%' " +
                    $"OR user_action LIKE '%{escaped}%' " +
                    $"OR module LIKE '%{escaped}%' " +
                    $"OR details LIKE '%{escaped}%')");
            }

            // Action filter (from comboBox1)
            if (!string.IsNullOrWhiteSpace(actionFilter))
            {
                string escapedAction = actionFilter.Replace("'", "''");
                filters.Add($"user_action = '{escapedAction}'");
            }

            // Date filter (from dateTimePicker3) – compare only by date
            if (dateFilter.HasValue)
            {
                DateTime start = dateFilter.Value.Date;
                DateTime end = start.AddDays(1);
                filters.Add(
                    $"log_timestamp >= #{start:yyyy-MM-dd}# AND log_timestamp < #{end:yyyy-MM-dd}#");
            }

            string combinedFilter = string.Join(" AND ", filters);

            DataView view = userLogsTable.DefaultView;
            view.RowFilter = combinedFilter;
            dataGridView2.DataSource = view;
        }

        // ============================================================
        //  BUTTON CLICK HANDLERS
        // ============================================================

        private void AddUserBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (loggedInUserId == 0)
                {
                    MessageBox.Show(
                        "Unable to determine current user. Please log in again.",
                        "Authentication Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                AddNewUser addUserForm = new AddNewUser(loggedInUserId);
                addUserForm.ShowDialog();
                
                // Refresh user list after adding a new user
                LoadUsers();
                LoadUserLogs(); // Also refresh logs
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening Add New User form: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ChangepassBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (loggedInUserId == 0)
                {
                    MessageBox.Show(
                        "Unable to determine current user. Please log in again.",
                        "Authentication Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                ChangePassword changePasswordForm = new ChangePassword(loggedInUserId);
                changePasswordForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening Change Password form: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void EditProfileBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (loggedInUserId == 0)
                {
                    MessageBox.Show(
                        "Unable to determine current user. Please log in again.",
                        "Authentication Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                UpdateUserProfile updateProfileForm = new UpdateUserProfile(loggedInUserId);
                updateProfileForm.ShowDialog();
                
                // Refresh user list after updating profile
                LoadUsers();
                LoadUserLogs(); // Also refresh logs

                // Refresh the logged-in user's full name in Form2 (parent form)
                RefreshForm2UserFullName();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error opening Update User Profile form: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Refreshes the logged-in user's full name in Form2 (parent form)
        /// </summary>
        private void RefreshForm2UserFullName()
        {
            try
            {
                // Find Form2 in the application's open forms
                Form2 parentForm = Application.OpenForms.OfType<Form2>().FirstOrDefault();
                
                if (parentForm != null)
                {
                    parentForm.RefreshLoggedInUserFullName();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to refresh Form2 user full name: {ex.Message}");
            }
        }

        // ============================================================
        //  CSV EXPORT FUNCTIONALITY
        // ============================================================

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export User Logs";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = $"user_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write header row
                        writer.WriteLine("Timestamp,User,Action,Module,Details,IP Address");

                        // Write data rows - access via column index for reliability
                        foreach (DataGridViewRow row in dataGridView2.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            // Access cells by column index (0-based)
                            string timestamp = EscapeForCsv(row.Cells[0].Value?.ToString());
                            string user = EscapeForCsv(row.Cells[1].Value?.ToString());
                            string action = EscapeForCsv(row.Cells[2].Value?.ToString());
                            string module = EscapeForCsv(row.Cells[3].Value?.ToString());
                            string details = EscapeForCsv(row.Cells[4].Value?.ToString());
                            string ipAddress = EscapeForCsv(row.Cells[5].Value?.ToString());

                            writer.WriteLine($"{timestamp},{user},{action},{module},{details},{ipAddress}");
                        }
                    }

                    MessageBox.Show("User logs exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Escapes a string for CSV format by handling commas, quotes, and newlines
        /// </summary>
        private string EscapeForCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            // If the value contains comma, quote, or newline, wrap it in quotes and escape internal quotes
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
