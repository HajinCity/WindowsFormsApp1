using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class OrsBursForm : Form
    {
        private class OrsBursRecord
        {
            public int Id { get; set; }
            public string SerialNo { get; set; }
            public DateTime Date { get; set; }
            public string FundCluster { get; set; }
            public string PoNo { get; set; }
            public string Payee { get; set; }
            public string Office { get; set; }
            public string ResponsibilityCenter { get; set; }
            public string ApprovingOfficer { get; set; }
            public string Amount { get; set; }
            public string Status { get; set; }
        }

        private readonly List<OrsBursRecord> orsBursCache = new List<OrsBursRecord>();

        public OrsBursForm()
        {
            InitializeComponent();
            addEntryBtn.Click += AddEntryBtn_Click;
            this.Load += OrsBursForm_Load;
            dataGridView2.CellContentClick += DataGridView2_CellContentClick;
            pictureBox2.Click += PictureBox2_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
            ParseRangeBtn.Click += ParseRangeBtn_Click;
            ExportCSV.Click += ExportToCSV_Click;
        }

        private void OrsBursForm_Load(object sender, EventArgs e)
        {
            // Set initial date range to show all data (wide range)
            dateTimePicker1.Value = DateTime.Today.AddYears(-10);
            dateTimePicker2.Value = DateTime.Today.AddYears(1);
            LoadOrsBursData();
        }

        private void AddEntryBtn_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddORSBURS())
            {
                addForm.ShowDialog(this);
                // Always refresh data after the form closes to show any new entries
                LoadOrsBursData();
            }
        }

        private void LoadOrsBursData()
        {
            try
            {
                orsBursCache.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT ora_burono, ora_serialno, date, fund_cluster, po_no, payee, office, 
                                     responsibility_center, approving_officer, payable_amount, status
                                     FROM ora_burono
                                     ORDER BY date DESC, ora_burono DESC";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orsBursCache.Add(new OrsBursRecord
                            {
                                Id = reader.GetInt32("ora_burono"),
                                SerialNo = reader["ora_serialno"]?.ToString(),
                                Date = reader["date"] == DBNull.Value ? DateTime.MinValue : reader.GetDateTime("date"),
                                FundCluster = reader["fund_cluster"]?.ToString(),
                                PoNo = reader["po_no"]?.ToString(),
                                Payee = reader["payee"]?.ToString(),
                                Office = reader["office"]?.ToString(),
                                ResponsibilityCenter = reader["responsibility_center"]?.ToString(),
                                ApprovingOfficer = reader["approving_officer"]?.ToString(),
                                Amount = reader["payable_amount"]?.ToString(),
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
                    $"Unable to load ORS-BURS data: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DisplayAllData()
        {
            dataGridView2.Rows.Clear();
            foreach (var entry in orsBursCache)
            {
                int rowIndex = dataGridView2.Rows.Add(
                    entry.SerialNo,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.FundCluster,
                    entry.PoNo,
                    entry.Payee,
                    entry.Office,
                    entry.ResponsibilityCenter,
                    entry.ApprovingOfficer,
                    FormatAmountDisplay(entry.Amount),
                    entry.Status);
                dataGridView2.Rows[rowIndex].Tag = entry.Id;
            }
        }

        private void DataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var row = dataGridView2.Rows[e.RowIndex];
            if (row?.Tag == null || !int.TryParse(row.Tag.ToString(), out int orsBursId))
            {
                return;
            }

            string columnName = dataGridView2.Columns[e.ColumnIndex].Name;
            if (columnName == "botedit")
            {
                using (var updateForm = new UpdateORSBURSForm(orsBursId))
                {
                    if (updateForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadOrsBursData();
                    }
                }
            }
            else if (columnName == "botview")
            {
                using (var viewForm = new ViewORSBURS(orsBursId))
                {
                    viewForm.ShowDialog(this);
                }
            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            ApplyOrsBursFilter();
        }

        private void ApplyOrsBursFilter()
        {
            string term = (textBox1.Text ?? string.Empty).Trim().ToLowerInvariant();
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;
            string selectedStatus = comboBox1.SelectedItem?.ToString();

            var filtered = orsBursCache.Where(entry =>
            {
                // Date range filter
                bool dateMatch = (entry.Date == DateTime.MinValue) ||
                                 (entry.Date.Date >= startDate && entry.Date.Date <= endDate);

                // Status filter
                bool statusMatch = string.IsNullOrEmpty(selectedStatus) ||
                                   (entry.Status ?? "").Equals(selectedStatus, StringComparison.OrdinalIgnoreCase);

                // Search term filter
                bool searchMatch = string.IsNullOrEmpty(term) ||
                                   (entry.SerialNo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                                   (entry.FundCluster ?? string.Empty).ToLowerInvariant().Contains(term) ||
                                   (entry.PoNo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                                   (entry.Payee ?? string.Empty).ToLowerInvariant().Contains(term) ||
                                   (entry.Office ?? string.Empty).ToLowerInvariant().Contains(term) ||
                                   (entry.ResponsibilityCenter ?? string.Empty).ToLowerInvariant().Contains(term) ||
                                   (entry.ApprovingOfficer ?? string.Empty).ToLowerInvariant().Contains(term);

                return dateMatch && statusMatch && searchMatch;
            });

            dataGridView2.Rows.Clear();
            foreach (var entry in filtered)
            {
                int rowIndex = dataGridView2.Rows.Add(
                    entry.SerialNo,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.FundCluster,
                    entry.PoNo,
                    entry.Payee,
                    entry.Office,
                    entry.ResponsibilityCenter,
                    entry.ApprovingOfficer,
                    FormatAmountDisplay(entry.Amount),
                    entry.Status);
                dataGridView2.Rows[rowIndex].Tag = entry.Id;
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

            // Apply the filters (date range, status, and search)
            ApplyOrsBursFilter();
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            // Refresh and display all data from ora_burono table
            dateTimePicker1.Value = DateTime.Today.AddYears(-10);
            dateTimePicker2.Value = DateTime.Today.AddYears(1);
            textBox1.Clear();
            comboBox1.SelectedIndex = -1; // Clear status filter
            LoadOrsBursData();
        }

        private void ExportToCSV_Click(object sender, EventArgs e)
        {
            if (dataGridView2.Rows.Count == 0)
            {
                MessageBox.Show("There is no data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export ORS-BURS Data";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = $"ors_burs_data_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write header row
                        writer.WriteLine("Serial No.,Date,Fund Cluster,PO No.,Payee,Office,Responsibility Center,Approving Officer,Total Amount,Status");

                        // Write data rows
                        foreach (DataGridViewRow row in dataGridView2.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string serialNo = EscapeForCsv(row.Cells["Column11"].Value?.ToString());
                            string date = EscapeForCsv(row.Cells["Column12"].Value?.ToString());
                            string fundCluster = EscapeForCsv(row.Cells["Column13"].Value?.ToString());
                            string poNo = EscapeForCsv(row.Cells["Column14"].Value?.ToString());
                            string payee = EscapeForCsv(row.Cells["Column15"].Value?.ToString());
                            string office = EscapeForCsv(row.Cells["Column16"].Value?.ToString());
                            string responsibilityCenter = EscapeForCsv(row.Cells["Column17"].Value?.ToString());
                            string approvingOfficer = EscapeForCsv(row.Cells["Column18"].Value?.ToString());
                            string amount = EscapeForCsv(row.Cells["Column19"].Value?.ToString());
                            string status = EscapeForCsv(row.Cells["Column20"].Value?.ToString());

                            writer.WriteLine($"{serialNo},{date},{fundCluster},{poNo},{payee},{office},{responsibilityCenter},{approvingOfficer},{amount},{status}");
                        }
                    }

                    MessageBox.Show("ORS-BURS data exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export ORS-BURS data: {ex.Message}", "Export Error",
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
