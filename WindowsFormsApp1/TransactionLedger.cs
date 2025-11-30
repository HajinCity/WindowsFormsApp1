using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class TransactionLedger : Form
    {
        private class SBLPORecord
        {
            public int Id { get; set; }
            public string PoNo { get; set; }
            public string Supplier { get; set; }
            public string PoAmount { get; set; }
            public string OraSerialNo { get; set; }
            public string ResponsibilityCode { get; set; }
            public string DevNo { get; set; }
            public string JevNo { get; set; }
            public string CheckNo { get; set; }
            public DateTime DatePaid { get; set; }
            public string AmountPaid { get; set; }
            public string Balance { get; set; }
            public string Status { get; set; }
        }

        private readonly List<SBLPORecord> sblPoCache = new List<SBLPORecord>();
        private bool suppressFilterEvents = false;
        private bool applyDateRangeFilter = false;

        public TransactionLedger()
        {
            InitializeComponent();
            InitializeFilterControls();
            this.Load += TransactionLedger_Load;
        }

        private void InitializeFilterControls()
        {
            // Initialize comboBox1 with status options
            if (comboBox1 != null)
            {
                comboBox1.Items.Clear();
                comboBox1.Items.Add("Fully Paid");
                comboBox1.Items.Add("Partially Paid");
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            }

            // Set up event handlers
            if (textBox1 != null)
            {
                textBox1.TextChanged += TextBox1_TextChanged;
            }

            if (EnterBtn != null)
            {
                EnterBtn.Click += EnterBtn_Click;
            }

            // Initialize date range to show all data (wide range)
            if (dateTimePicker1 != null && dateTimePicker2 != null)
            {
                suppressFilterEvents = true;
                try
                {
                    dateTimePicker1.Value = DateTime.Today.AddYears(-10);
                    dateTimePicker2.Value = DateTime.Today.AddYears(1);
                }
                finally
                {
                    suppressFilterEvents = false;
                }
            }
        }

        private void TransactionLedger_Load(object sender, EventArgs e)
        {
            LoadSBLPOData();
        }

        private void LoadSBLPOData()
        {
            try
            {
                sblPoCache.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT sbl_id, po_no, supplier, po_amount, ora_serialno, 
                                           responsibility_code, dev_no, jev_no, checkNo, 
                                           date_paid, amount_paid, balance, status
                                    FROM sbl_po
                                    ORDER BY date_paid DESC, sbl_id DESC";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            sblPoCache.Add(new SBLPORecord
                            {
                                Id = reader.GetInt32("sbl_id"),
                                PoNo = reader["po_no"]?.ToString() ?? "",
                                Supplier = reader["supplier"]?.ToString() ?? "",
                                PoAmount = reader["po_amount"]?.ToString() ?? "",
                                OraSerialNo = reader["ora_serialno"]?.ToString() ?? "",
                                ResponsibilityCode = reader["responsibility_code"]?.ToString() ?? "",
                                DevNo = reader["dev_no"]?.ToString() ?? "",
                                JevNo = reader["jev_no"]?.ToString() ?? "",
                                CheckNo = reader["checkNo"]?.ToString() ?? "",
                                DatePaid = reader["date_paid"] == DBNull.Value ? DateTime.MinValue : reader.GetDateTime("date_paid"),
                                AmountPaid = reader["amount_paid"]?.ToString() ?? "",
                                Balance = reader["balance"]?.ToString() ?? "",
                                Status = reader["status"]?.ToString() ?? ""
                            });
                        }
                    }
                }

                DisplayAllData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load transaction ledger data: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void DisplayAllData()
        {
            dataGridView1.Rows.Clear();
            foreach (var entry in sblPoCache)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.PoNo,
                    entry.Supplier,
                    FormatAmountDisplay(entry.PoAmount),
                    entry.OraSerialNo,
                    entry.ResponsibilityCode,
                    entry.DevNo,
                    entry.JevNo,
                    entry.CheckNo,
                    entry.DatePaid == DateTime.MinValue ? "" : entry.DatePaid.ToShortDateString(),
                    FormatAmountDisplay(entry.AmountPaid),
                    FormatAmountDisplay(entry.Balance),
                    entry.Status);
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
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

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            if (!suppressFilterEvents)
            {
                ApplyFilter();
            }
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!suppressFilterEvents)
            {
                ApplyFilter();
            }
        }

        private void EnterBtn_Click(object sender, EventArgs e)
        {
            // Validate date range
            if (dateTimePicker1 != null && dateTimePicker2 != null)
            {
                if (dateTimePicker1.Value.Date > dateTimePicker2.Value.Date)
                {
                    MessageBox.Show(
                        "Start date cannot be greater than end date. Please adjust the date range.",
                        "Invalid Date Range",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            // Enable date range filtering when EnterBtn is clicked
            applyDateRangeFilter = true;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string searchTerm = (textBox1?.Text ?? string.Empty).Trim().ToLowerInvariant();
            string selectedStatus = comboBox1?.SelectedItem?.ToString();
            DateTime? startDate = null;
            DateTime? endDate = null;

            // Get date range only if EnterBtn was clicked (applyDateRangeFilter flag is true)
            if (applyDateRangeFilter && dateTimePicker1 != null && dateTimePicker2 != null)
            {
                startDate = dateTimePicker1.Value.Date;
                endDate = dateTimePicker2.Value.Date;
            }

            var filtered = sblPoCache.Where(entry =>
            {
                // Text search filter (searches in all specified columns)
                bool textMatch = string.IsNullOrEmpty(searchTerm);
                if (!textMatch)
                {
                    textMatch = (entry.PoNo ?? string.Empty).ToLowerInvariant().Contains(searchTerm) ||
                               (entry.Supplier ?? string.Empty).ToLowerInvariant().Contains(searchTerm) ||
                               (entry.PoAmount ?? string.Empty).Replace(",", "").ToLowerInvariant().Contains(searchTerm) ||
                               (entry.OraSerialNo ?? string.Empty).ToLowerInvariant().Contains(searchTerm) ||
                               (entry.ResponsibilityCode ?? string.Empty).ToLowerInvariant().Contains(searchTerm) ||
                               (entry.DevNo ?? string.Empty).ToLowerInvariant().Contains(searchTerm) ||
                               (entry.JevNo ?? string.Empty).ToLowerInvariant().Contains(searchTerm) ||
                               (entry.CheckNo ?? string.Empty).ToLowerInvariant().Contains(searchTerm);
                }

                // Status filter
                bool statusMatch = string.IsNullOrEmpty(selectedStatus) ||
                                  (entry.Status ?? "").Equals(selectedStatus, StringComparison.OrdinalIgnoreCase);

                // Date range filter
                bool dateMatch = true;
                if (startDate.HasValue && endDate.HasValue)
                {
                    dateMatch = (entry.DatePaid == DateTime.MinValue) ||
                               (entry.DatePaid.Date >= startDate.Value && entry.DatePaid.Date <= endDate.Value);
                }

                return textMatch && statusMatch && dateMatch;
            });

            dataGridView1.Rows.Clear();
            foreach (var entry in filtered)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.PoNo,
                    entry.Supplier,
                    FormatAmountDisplay(entry.PoAmount),
                    entry.OraSerialNo,
                    entry.ResponsibilityCode,
                    entry.DevNo,
                    entry.JevNo,
                    entry.CheckNo,
                    entry.DatePaid == DateTime.MinValue ? "" : entry.DatePaid.ToShortDateString(),
                    FormatAmountDisplay(entry.AmountPaid),
                    FormatAmountDisplay(entry.Balance),
                    entry.Status);
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
           
        }
    }
}
