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
    public partial class IARForm : Form
    {
        private class IarRecord
        {
            public int Id { get; set; }
            public string IarNo { get; set; }
            public DateTime Date { get; set; }
            public string Supplier { get; set; }
            public string PoNumber { get; set; }
            public string RequisitioningOffice { get; set; }
            public string TotalAmount { get; set; }
        }

        private readonly List<IarRecord> iarCache = new List<IarRecord>();
        private bool suppressFilterEvents = false;
        private bool hasInitializedRange = false;
        private int loggedInUserId = 0;

        public IARForm()
        {
            InitializeComponent();
            addIARbtn.Click += AddIARbtn_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
            ExportToCSV.Click += ExportToCSV_Click;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            pictureBox2.Click += PictureBox2_Click;
            ParseRangeBtn.Click += ParseRangeBtn_Click;
            this.Load += IARForm_Load;
        }

        public void SetLoggedInUserId(int userId)
        {
            loggedInUserId = userId;
        }

        private void IARForm_Load(object sender, EventArgs e)
        {
            // Set initial date range to show all data (wide range)
            if (!hasInitializedRange)
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
                    hasInitializedRange = true;
                }
            }
            LoadIarReports();
        }

        private void AddIARbtn_Click(object sender, EventArgs e)
        {
            if (loggedInUserId == 0)
            {
                MessageBox.Show("Unable to determine current user. Please log in again.", "Authentication Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var addIARForm = new AddIARForm(loggedInUserId))
            {
                if (addIARForm.ShowDialog(this) == DialogResult.OK)
                {
                    LoadIarReports();
                }
            }
        }

        private void LoadIarReports()
        {
            try
            {
                iarCache.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT iar_id, iar_no, date_iar, supplier, po_no, requisitioning_office, total_amount
                                     FROM inspection_acceptance_report
                                     ORDER BY date_iar DESC, iar_no";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            iarCache.Add(new IarRecord
                            {
                                Id = reader.GetInt32("iar_id"),
                                IarNo = reader["iar_no"]?.ToString(),
                                Date = reader["date_iar"] == DBNull.Value ? DateTime.MinValue : reader.GetDateTime("date_iar"),
                                Supplier = reader["supplier"]?.ToString(),
                                PoNumber = reader["po_no"]?.ToString(),
                                RequisitioningOffice = reader["requisitioning_office"]?.ToString(),
                                TotalAmount = reader["total_amount"]?.ToString()
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
                    $"Unable to load inspection reports: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            ApplyIarFilter();
        }

        private void DisplayAllData()
        {
            // Display all data from inspection_acceptance_report table without any filtering
            dataGridView1.Rows.Clear();
            foreach (var entry in iarCache)
        {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.IarNo,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.Supplier,
                    entry.PoNumber,
                    entry.RequisitioningOffice,
                    FormatAmountDisplay(entry.TotalAmount));
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
        }

        private void ApplyIarFilter()
        {
            string term = (textBox1.Text ?? string.Empty).Trim().ToLowerInvariant();
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;

            var filtered = iarCache.Where(entry =>
                ((entry.Date == DateTime.MinValue) ||
                 (entry.Date.Date >= startDate && entry.Date.Date <= endDate)) &&
                (string.IsNullOrEmpty(term) ||
                 (entry.IarNo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                 (entry.Supplier ?? string.Empty).ToLowerInvariant().Contains(term) ||
                 (entry.PoNumber ?? string.Empty).ToLowerInvariant().Contains(term)));

            dataGridView1.Rows.Clear();
            foreach (var entry in filtered)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.IarNo,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.Supplier,
                    entry.PoNumber,
                    entry.RequisitioningOffice,
                    FormatAmountDisplay(entry.TotalAmount));
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

            // Apply the date range filter
            ApplyIarFilter();
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            // Refresh and display all data from inspection_acceptance_report table
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
            LoadIarReports();
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
                saveFileDialog.Title = "Export IAR Data";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = $"iar_data_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write header row
                        writer.WriteLine("IAR Number,Date,Supplier,PO Number,Requisitioning Office,Total Amount");

                        // Write data rows
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string iarNo = EscapeForCsv(row.Cells[0].Value?.ToString());
                            string date = EscapeForCsv(row.Cells[1].Value?.ToString());
                            string supplier = EscapeForCsv(row.Cells[2].Value?.ToString());
                            string poNumber = EscapeForCsv(row.Cells[3].Value?.ToString());
                            string requisitioningOffice = EscapeForCsv(row.Cells[4].Value?.ToString());
                            string totalAmount = EscapeForCsv(row.Cells[5].Value?.ToString());

                            writer.WriteLine($"{iarNo},{date},{supplier},{poNumber},{requisitioningOffice},{totalAmount}");
                        }
                    }

                    MessageBox.Show("IAR data exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export IAR data: {ex.Message}", "Export Error",
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

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var row = dataGridView1.Rows[e.RowIndex];
            if (row?.Tag == null || !int.TryParse(row.Tag.ToString(), out int iarId))
            {
                return;
            }

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName == "BtnEdit")
            {
                using (var updateForm = new UpdateIARForm(iarId))
                {
                    if (updateForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadIarReports();
                    }
                }
            }
            else if (columnName == "BtnView")
            {
                using (var viewForm = new ViewIAR(iarId))
                {
                    viewForm.ShowDialog(this);
                }
            }
        }
    }
}
