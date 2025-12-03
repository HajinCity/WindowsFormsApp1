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

        public UserManagement()
        {
            InitializeComponent();
            LoadUsers();
            LoadUserLogs();
            textBox2.TextChanged += TextBox2_TextChanged;
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
                        DataTable table = new DataTable();
                        adapter.Fill(table);

                        // We are using designer-created columns, so don't auto-generate.
                        dataGridView2.AutoGenerateColumns = false;

                        // Map each DataGridView column to the corresponding DataTable column.
                        // Make sure these column names (dataGridViewTextBoxColumnX) match your designer.
                        dataGridViewTextBoxColumn1.DataPropertyName = "log_timestamp";   // Timestamp
                        dataGridViewTextBoxColumn2.DataPropertyName = "user_fullname";   // User (full name)
                        dataGridViewTextBoxColumn3.DataPropertyName = "user_action";     // Action
                        dataGridViewTextBoxColumn4.DataPropertyName = "module";          // Module
                        dataGridViewTextBoxColumn5.DataPropertyName = "details";         // Details

                        dataGridView2.DataSource = table;
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
    }
}
