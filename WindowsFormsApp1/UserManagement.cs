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

        public UserManagement()
        {
            InitializeComponent();
            LoadUsers();
            LoadUserLogs();
            textBox2.TextChanged += TextBox2_TextChanged;
            textBox3.TextChanged += TextBox3_TextChanged;
            EnterBtn.Click += EnterBtn_Click;
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
                            ul.details            AS details
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
    }
}
