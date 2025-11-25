using System;
using System.Collections.Generic;
using System.Data;
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

        public IARForm()
        {
            InitializeComponent();
            addIARbtn.Click += AddIARbtn_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
            dateTimePicker1.ValueChanged += DateFilter_ValueChanged;
            dateTimePicker2.ValueChanged += DateFilter_ValueChanged;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            this.Load += IARForm_Load;
        }

        private void IARForm_Load(object sender, EventArgs e)
        {
            LoadIarReports();
        }

        private void AddIARbtn_Click(object sender, EventArgs e)
        {
            using (var addIARForm = new AddIARForm())
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

                if (!hasInitializedRange)
                {
                    SetInitialDateRange();
                    hasInitializedRange = true;
                }

                ApplyIarFilter();
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

        private void DateFilter_ValueChanged(object sender, EventArgs e)
        {
            if (suppressFilterEvents)
            {
                return;
            }

            if (dateTimePicker1.Value.Date > dateTimePicker2.Value.Date)
            {
                dateTimePicker2.Value = dateTimePicker1.Value.Date;
            }

            ApplyIarFilter();
        }

        private void SetInitialDateRange()
        {
            suppressFilterEvents = true;
            try
            {
                var datedEntries = iarCache.Where(entry => entry.Date != DateTime.MinValue).ToList();
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
                    entry.TotalAmount);
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
            }
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
        }
    }
}
