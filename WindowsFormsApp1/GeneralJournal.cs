using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class GeneralJournal : Form
    {
        private class JournalEntryRecord
        {
            public int Id { get; set; }
            public string GJNumber { get; set; }
            public DateTime Date { get; set; }
            public string Particulars { get; set; }
            public string UacsCode { get; set; }
            public string Amount { get; set; }
        }

        private readonly List<JournalEntryRecord> journalCache = new List<JournalEntryRecord>();
        private bool suppressFilterEvents = false;
        private bool hasInitializedDateRange = false;
        private int loggedInUserId = 0;

        public GeneralJournal()
        {
            InitializeComponent();
            addEntryBtn.Click += AddEntryBtn_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
            ExportToCSV.Click += ExportToCSV_Click;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            pictureBox2.Click += PictureBox2_Click;
            ParseRangeBtn.Click += ParseRange_Click;
            this.Load += GeneralJournal_Load;
        }

        public void SetLoggedInUserId(int userId)
        {
            loggedInUserId = userId;
        }

        private void GeneralJournal_Load(object sender, EventArgs e)
        {
            // Set initial date range to show all data (wide range)
            if (!hasInitializedDateRange)
            {
                suppressFilterEvents = true;
                try
                {
                    dateTimePicker1.Value = DateTime.Today.AddYears(-10); // Start date: 10 years ago
                    dateTimePicker2.Value = DateTime.Today.AddYears(1); // End date: 1 year from now
                }
                finally
                {
                    suppressFilterEvents = false;
                    hasInitializedDateRange = true;
                }
            }
            LoadJournalEntries();
        }

        private void AddEntryBtn_Click(object sender, EventArgs e)
        {
            if (loggedInUserId == 0)
            {
                MessageBox.Show("Unable to determine current user. Please log in again.", "Authentication Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var addEntryForm = new AddNewJournalEntry(loggedInUserId))
            {
                if (addEntryForm.ShowDialog(this) == DialogResult.OK)
                {
                    LoadJournalEntries();
                }
            }
        }

        private void LoadJournalEntries()
        {
            try
            {
                journalCache.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT gj_id, gj_no, particulars, uacs_code, amount, date 
                                     FROM general_journal
                                     ORDER BY date DESC, gj_no";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            journalCache.Add(new JournalEntryRecord
                            {
                                Id = reader.GetInt32("gj_id"),
                                GJNumber = reader["gj_no"]?.ToString(),
                                Particulars = reader["particulars"]?.ToString(),
                                UacsCode = reader["uacs_code"]?.ToString(),
                                Amount = reader["amount"]?.ToString(),
                                Date = reader["date"] == DBNull.Value ? DateTime.MinValue : reader.GetDateTime("date")
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
                    $"Unable to load journal entries: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            ApplyJournalFilter();
        }

        private void ParseRange_Click(object sender, EventArgs e)
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

            // Apply the date range filter
            ApplyJournalFilter();
        }

        private void SetInitialDateRange()
        {
            suppressFilterEvents = true;
            try
            {
                var datedEntries = journalCache.Where(entry => entry.Date != DateTime.MinValue).ToList();
                DateTime today = DateTime.Today;

                if (datedEntries.Count > 0)
                {
                    dateTimePicker1.Value = datedEntries.Min(entry => entry.Date).Date;
                    dateTimePicker2.Value = datedEntries.Max(entry => entry.Date).Date;
                }
                else
                {
                    dateTimePicker1.Value = today.AddYears(-1);
                    dateTimePicker2.Value = today;
                }
            }
            finally
            {
                suppressFilterEvents = false;
            }
        }

        private void DisplayAllData()
        {
            // Display all data from general_journal table without any filtering
            dataGridView1.Rows.Clear();
            foreach (var entry in journalCache)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.GJNumber,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.Particulars,
                    entry.UacsCode,
                    FormatAmountDisplay(entry.Amount));
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
        }

        private void ApplyJournalFilter()
        {
            string term = (textBox1.Text ?? string.Empty).Trim().ToLowerInvariant();
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;

            var filtered = journalCache.Where(entry =>
                ((entry.Date == DateTime.MinValue) ||
                 (entry.Date.Date >= startDate && entry.Date.Date <= endDate)) &&
                (string.IsNullOrEmpty(term) ||
                 (entry.GJNumber ?? string.Empty).ToLowerInvariant().Contains(term) ||
                 (entry.Particulars ?? string.Empty).ToLowerInvariant().Contains(term) ||
                 (entry.UacsCode ?? string.Empty).ToLowerInvariant().Contains(term)));

            dataGridView1.Rows.Clear();
            foreach (var entry in filtered)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.GJNumber,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.Particulars,
                    entry.UacsCode,
                    FormatAmountDisplay(entry.Amount));
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            // Refresh and display all data from general_journal table
            suppressFilterEvents = true;
            try
            {
                // Reset date pickers to wide range to show all data
                dateTimePicker1.Value = DateTime.Today.AddYears(-10);
                dateTimePicker2.Value = DateTime.Today.AddYears(1);
            }
            finally
            {
                suppressFilterEvents = false;
            }
            
            // Clear search text
            textBox1.Clear();
            
            // Reload and display all data
            LoadJournalEntries();
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var row = dataGridView1.Rows[e.RowIndex];
            if (row?.Tag == null || !int.TryParse(row.Tag.ToString(), out int journalId))
            {
                return;
            }

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName == "BtnEdit")
            {
                using (var updateForm = new UpdateGJ(journalId))
                {
                    if (updateForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadJournalEntries();
                    }
                }
            }
            else if (columnName == "BtnView")
            {
                if (loggedInUserId == 0)
                {
                    MessageBox.Show("Unable to determine current user. Please log in again.", "Authentication Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var viewForm = new ViewGJEntry(journalId, loggedInUserId))
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
                saveFileDialog.Title = "Export General Journal";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = $"general_journal_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("GJ Number,Date,Particulars,UACS Code,Amount");

                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string gj = EscapeForCsv(row.Cells[0].Value?.ToString());
                            string date = EscapeForCsv(row.Cells[1].Value?.ToString());
                            string particulars = EscapeForCsv(row.Cells[2].Value?.ToString());
                            string uacs = EscapeForCsv(row.Cells[3].Value?.ToString());
                            string amount = EscapeForCsv(row.Cells[4].Value?.ToString());

                            writer.WriteLine($"{gj},{date},{particulars},{uacs},{amount}");
                        }
                    }

                    MessageBox.Show("Journal entries exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export journal entries: {ex.Message}", "Export Error",
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

        private string FormatAmountDisplay(string amountValue)
        {
            if (string.IsNullOrWhiteSpace(amountValue))
            {
                return amountValue ?? string.Empty;
            }

            string clean = amountValue.Replace(",", "").Trim();
            if (decimal.TryParse(clean, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out decimal numericValue))
            {
                string formattedInteger = string.Format(CultureInfo.InvariantCulture, "{0:N0}", Math.Truncate(numericValue));
                int decimalIndex = clean.IndexOf('.');
                string fractionalPart = decimalIndex >= 0 ? clean.Substring(decimalIndex) : string.Empty;
                return formattedInteger + fractionalPart;
            }

            return amountValue;
        }
    }
}
