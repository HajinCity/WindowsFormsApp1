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
    public partial class DEVForm : Form
    {
        private class DEVRecord
        {
            public int Id { get; set; }
            public string DevNo { get; set; }
            public DateTime Date { get; set; }
            public string OrsBursNo { get; set; }
            public string JevNo { get; set; }
            public string Payee { get; set; }
            public string Office { get; set; }
            public string MOP { get; set; }
            public string GrossAmount { get; set; }
            public string Deductions { get; set; }
            public string NetAmount { get; set; }
            public string TaxType { get; set; }
        }

        private readonly List<DEVRecord> devCache = new List<DEVRecord>();
        private bool suppressFilterEvents = false;
        private bool hasInitializedRange = false;
        private int loggedInUserId = 0;

        public DEVForm()
        {
            InitializeComponent();
            addDEVbtn.Click += addDEVbtn_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
            ParseRangeBtn.Click += ParseRangeBtn_Click;
            pictureBox2.Click += PictureBox2_Click;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            this.Load += DEVForm_Load;
            
            // Wire up CSV export button if it exists
            if (ExportToCSV != null)
            {
                ExportToCSV.Click += ExportToCSV_Click;
            }
        }

        public void SetLoggedInUserId(int userId)
        {
            loggedInUserId = userId;
        }

        private void DEVForm_Load(object sender, EventArgs e)
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
            LoadDEVData();
        }

        private void addDEVbtn_Click(object sender, EventArgs e)
        {
            if (loggedInUserId == 0)
            {
                MessageBox.Show("Unable to determine current user. Please log in again.", "Authentication Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var addForm = new AddDevEntry(loggedInUserId))
            {
                if (addForm.ShowDialog(this) == DialogResult.OK)
                {
                    LoadDEVData();
                }
            }
        }

        private void LoadDEVData()
        {
            try
            {
                devCache.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT dev_id, dev_no, date, ora_serialno, jev_no, payee, 
                                            responsibility_center, mode_of_payment, gross_amount, 
                                            deductions, net_amount, tax_type
                                     FROM dev
                                     ORDER BY date DESC, dev_id DESC";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            devCache.Add(new DEVRecord
                            {
                                Id = reader.GetInt32("dev_id"),
                                DevNo = reader["dev_no"]?.ToString() ?? "",
                                Date = reader["date"] == DBNull.Value ? DateTime.MinValue : reader.GetDateTime("date"),
                                OrsBursNo = reader["ora_serialno"]?.ToString() ?? "",
                                JevNo = reader["jev_no"]?.ToString() ?? "",
                                Payee = reader["payee"]?.ToString() ?? "",
                                Office = reader["responsibility_center"]?.ToString() ?? "",
                                MOP = reader["mode_of_payment"]?.ToString() ?? "",
                                GrossAmount = reader["gross_amount"]?.ToString() ?? "",
                                Deductions = reader["deductions"]?.ToString() ?? "",
                                NetAmount = reader["net_amount"]?.ToString() ?? "",
                                TaxType = reader["tax_type"]?.ToString() ?? ""
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
                    $"Unable to load DEV data: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DisplayAllData()
        {
            dataGridView1.Rows.Clear();
            foreach (var entry in devCache)
            {
                int rowIndex = dataGridView1.Rows.Add();
                dataGridView1.Rows[rowIndex].Cells["Column1"].Value = entry.DevNo; // DEV No.
                dataGridView1.Rows[rowIndex].Cells["Column2"].Value = entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(); // Date
                dataGridView1.Rows[rowIndex].Cells["Column3"].Value = entry.OrsBursNo; // ORS BURS
                dataGridView1.Rows[rowIndex].Cells["Column4"].Value = entry.JevNo; // JEV No
                dataGridView1.Rows[rowIndex].Cells["Column5"].Value = entry.Payee; // Payee
                dataGridView1.Rows[rowIndex].Cells["Column6"].Value = entry.Office; // Office
                dataGridView1.Rows[rowIndex].Cells["Column7"].Value = entry.MOP; // MOP
                dataGridView1.Rows[rowIndex].Cells["Column8"].Value = entry.TaxType; // Tax Type
                dataGridView1.Rows[rowIndex].Cells["Column9"].Value = FormatAmountDisplay(entry.GrossAmount); // Gross Amount
                dataGridView1.Rows[rowIndex].Cells["Column10"].Value = FormatAmountDisplay(entry.Deductions); // Deductions
                dataGridView1.Rows[rowIndex].Cells["Column11"].Value = FormatAmountDisplay(entry.NetAmount); // Net Amount
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            ApplyDEVFilter();
        }

        private void ApplyDEVFilter()
        {
            string term = (textBox1.Text ?? string.Empty).Trim().ToLowerInvariant();
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;

            var filtered = devCache.Where(entry =>
                ((entry.Date == DateTime.MinValue) ||
                 (entry.Date.Date >= startDate && entry.Date.Date <= endDate)) &&
                (string.IsNullOrEmpty(term) ||
                 (entry.DevNo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                 (entry.OrsBursNo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                 (entry.JevNo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                 (entry.Payee ?? string.Empty).ToLowerInvariant().Contains(term)));

            dataGridView1.Rows.Clear();
            foreach (var entry in filtered)
            {
                int rowIndex = dataGridView1.Rows.Add();
                dataGridView1.Rows[rowIndex].Cells["Column1"].Value = entry.DevNo; // DEV No.
                dataGridView1.Rows[rowIndex].Cells["Column2"].Value = entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(); // Date
                dataGridView1.Rows[rowIndex].Cells["Column3"].Value = entry.OrsBursNo; // ORS BURS
                dataGridView1.Rows[rowIndex].Cells["Column4"].Value = entry.JevNo; // JEV No
                dataGridView1.Rows[rowIndex].Cells["Column5"].Value = entry.Payee; // Payee
                dataGridView1.Rows[rowIndex].Cells["Column6"].Value = entry.Office; // Office
                dataGridView1.Rows[rowIndex].Cells["Column7"].Value = entry.MOP; // MOP
                dataGridView1.Rows[rowIndex].Cells["Column8"].Value = entry.TaxType; // Tax Type
                dataGridView1.Rows[rowIndex].Cells["Column9"].Value = FormatAmountDisplay(entry.GrossAmount); // Gross Amount
                dataGridView1.Rows[rowIndex].Cells["Column10"].Value = FormatAmountDisplay(entry.Deductions); // Deductions
                dataGridView1.Rows[rowIndex].Cells["Column11"].Value = FormatAmountDisplay(entry.NetAmount); // Net Amount
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
            ApplyDEVFilter();
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            // Refresh and display all data from dev table
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
            LoadDEVData();
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
            if (row?.Tag == null || !int.TryParse(row.Tag.ToString(), out int devId))
            {
                return;
            }

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName == "BtnEdit")
            {
                using (var updateForm = new UpdateDEV(devId))
                {
                    if (updateForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadDEVData();
                    }
                }
            }
            else if (columnName == "BtnView")
            {
                using (var viewForm = new ViewDEV(devId))
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
                saveFileDialog.Title = "Export DEV Data";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = $"dev_data_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write header row
                        writer.WriteLine("DEV No.,Date,ORS BURS,JEV No,Payee,Office,MOP,Tax Type,Gross Amount,Deductions,Net Amount");

                        // Write data rows
                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string devNo = EscapeForCsv(row.Cells["Column1"].Value?.ToString());
                            string date = EscapeForCsv(row.Cells["Column2"].Value?.ToString());
                            string orsBurs = EscapeForCsv(row.Cells["Column3"].Value?.ToString());
                            string jevNo = EscapeForCsv(row.Cells["Column4"].Value?.ToString());
                            string payee = EscapeForCsv(row.Cells["Column5"].Value?.ToString());
                            string office = EscapeForCsv(row.Cells["Column6"].Value?.ToString());
                            string mop = EscapeForCsv(row.Cells["Column7"].Value?.ToString());
                            string taxType = EscapeForCsv(row.Cells["Column8"].Value?.ToString());
                            string grossAmount = EscapeForCsv(row.Cells["Column9"].Value?.ToString());
                            string deductions = EscapeForCsv(row.Cells["Column10"].Value?.ToString());
                            string netAmount = EscapeForCsv(row.Cells["Column11"].Value?.ToString());

                            writer.WriteLine($"{devNo},{date},{orsBurs},{jevNo},{payee},{office},{mop},{taxType},{grossAmount},{deductions},{netAmount}");
                        }
                    }

                    MessageBox.Show("DEV data exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export DEV data: {ex.Message}", "Export Error",
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
