using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class JEVForm : Form
    {
        private class JEVRecord
        {
            public int Id { get; set; }
            public string JevNo { get; set; }
            public DateTime Date { get; set; }
            public string UacsCode { get; set; }
            public string Account { get; set; }
            public string Particulars { get; set; }
        }

        private readonly List<JEVRecord> jevCache = new List<JEVRecord>();

        public JEVForm()
        {
            InitializeComponent();
            addJEVbtn.Click += AddJEVbtn_Click;
            this.Load += JEVForm_Load;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            textBox2.TextChanged += TextBox2_TextChanged;
            ParseRangeBtn.Click += ParseRangeBtn_Click;
            pictureBox2.Click += PictureBox2_Click;
            ExportToCSV.Click += ExportToCSV_Click;
        }

        private void JEVForm_Load(object sender, EventArgs e)
        {
            // Set initial date range to show all data (wide range)
            dateTimePicker1.Value = DateTime.Today.AddYears(-10);
            dateTimePicker2.Value = DateTime.Today.AddYears(1);
            LoadJEVData();
        }

        private void AddJEVbtn_Click(object sender, EventArgs e)
        {
            using (var addJevEntry = new AddJEVEntry())
            {
                if (addJevEntry.ShowDialog(this) == DialogResult.OK)
                {
                    // Refresh data after new entry is added
                    LoadJEVData();
                }
            }
        }

        private void LoadJEVData()
        {
            try
            {
                jevCache.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT jev_id, jev_no, date, uacs_code, account, particulars
                                     FROM jev
                                     ORDER BY date DESC, jev_id DESC";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jevCache.Add(new JEVRecord
                            {
                                Id = reader.GetInt32("jev_id"),
                                JevNo = reader["jev_no"]?.ToString() ?? "",
                                Date = reader["date"] == DBNull.Value ? DateTime.MinValue : reader.GetDateTime("date"),
                                UacsCode = reader["uacs_code"]?.ToString() ?? "",
                                Account = reader["account"]?.ToString() ?? "",
                                Particulars = reader["particulars"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                // Display all data
                DisplayAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load JEV data: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DisplayAllData()
        {
            dataGridView1.Rows.Clear();
            foreach (var entry in jevCache)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.JevNo,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.UacsCode,
                    entry.Account,
                    entry.Particulars);
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            ApplyJEVFilter();
        }

        private void ApplyJEVFilter()
        {
            string term = (textBox2.Text ?? string.Empty).Trim().ToLowerInvariant();
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;

            var filtered = jevCache.Where(entry =>
            {
                // Date range filter
                bool dateMatch = (entry.Date == DateTime.MinValue) ||
                                 (entry.Date.Date >= startDate && entry.Date.Date <= endDate);

                // Search term filter (JEV No., UACS, Account)
                bool searchMatch = string.IsNullOrEmpty(term) ||
                                   (entry.JevNo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                                   (entry.UacsCode ?? string.Empty).ToLowerInvariant().Contains(term) ||
                                   (entry.Account ?? string.Empty).ToLowerInvariant().Contains(term);

                return dateMatch && searchMatch;
            });

            dataGridView1.Rows.Clear();
            foreach (var entry in filtered)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.JevNo,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.UacsCode,
                    entry.Account,
                    entry.Particulars);
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
        }

        private void ParseRangeBtn_Click(object sender, EventArgs e)
        {
            // Validate date range
            if (dateTimePicker1.Value.Date > dateTimePicker2.Value.Date)
            {
                MessageBox.Show(
                    "Start date cannot be greater than end date. Please adjust the date range.",
                    "Invalid Date Range",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Apply the filters (date range and search)
            ApplyJEVFilter();
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            // Reset date pickers to wide range to show all data
            dateTimePicker1.Value = DateTime.Today.AddYears(-10);
            dateTimePicker2.Value = DateTime.Today.AddYears(1);

            // Clear search text
            textBox2.Text = "";

            // Reload all data from database
            LoadJEVData();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var row = dataGridView1.Rows[e.RowIndex];
            if (row?.Tag == null || !int.TryParse(row.Tag.ToString(), out int jevId))
            {
                return;
            }

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName == "BtnEdit")
            {
                using (var updateForm = new UpdateJEV(jevId))
                {
                    if (updateForm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Refresh data after update
                        LoadJEVData();
                    }
                }
            }
            else if (columnName == "BtnView")
            {
                using (var viewForm = new ViewJEV(jevId))
                {
                    viewForm.ShowDialog(this);
                }
            }
        }

        private void ExportToCSV_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export JEV Data";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = $"jev_data_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write header row
                        writer.WriteLine("JEV No.,Date,UACS,Account,Particulars");

                        // Write data rows
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string jevNo = EscapeForCsv(row.Cells[0].Value?.ToString());
                            string date = EscapeForCsv(row.Cells[1].Value?.ToString());
                            string uacs = EscapeForCsv(row.Cells[2].Value?.ToString());
                            string account = EscapeForCsv(row.Cells[3].Value?.ToString());
                            string particulars = EscapeForCsv(row.Cells[4].Value?.ToString());

                            writer.WriteLine($"{jevNo},{date},{uacs},{account},{particulars}");
                        }
                    }

                    MessageBox.Show("JEV data exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export JEV data: {ex.Message}", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string EscapeForCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n");
            string escaped = value.Replace("\"", "\"\"");
            return mustQuote ? $"\"{escaped}\"" : escaped;
        }
    }
}
