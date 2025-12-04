using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class SupplierForm : Form
    {
        private class SupplierRecord
        {
            public int SupplierId { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string ContactPerson { get; set; }
            public string ContactInfo { get; set; }
            public string BankName { get; set; }
        }

        private readonly List<SupplierRecord> supplierCache = new List<SupplierRecord>();
        private int loggedInUserId = 0;

        public SupplierForm()
        {
            InitializeComponent();
            addSupplierBtn.Click += AddSupplierBtn_Click;
            this.Load += SupplierForm_Load;
            dataGridView2.CellContentClick += DataGridView1_CellContentClick;
            textBox1.TextChanged += TextBox1_TextChanged;
            ExportToCSV.Click += ExportToCSV_Click;
        }

        public void SetLoggedInUserId(int userId)
        {
            loggedInUserId = userId;
        }

        private void SupplierForm_Load(object sender, EventArgs e)
        {
            LoadSuppliers();
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
                saveFileDialog.Title = "Export Suppliers";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = $"suppliers_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("Supplier Name,Address,Contact Person,Contact Info,Bank Name");

                        foreach (DataGridViewRow row in dataGridView2.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string name = EscapeForCsv(row.Cells["Column6"].Value?.ToString());
                            string address = EscapeForCsv(row.Cells["Column7"].Value?.ToString());
                            string contactPerson = EscapeForCsv(row.Cells["Column8"].Value?.ToString());
                            string contactInfo = EscapeForCsv(row.Cells["Column9"].Value?.ToString());
                            string bankName = EscapeForCsv(row.Cells["Column10"].Value?.ToString());

                            writer.WriteLine($"{name},{address},{contactPerson},{contactInfo},{bankName}");
                        }
                    }

                    MessageBox.Show("Suppliers exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export suppliers: {ex.Message}", "Export Error",
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

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            ApplySupplierFilter(textBox1.Text);
        }

        private void LoadSuppliers()
        {
            try
            {
                supplierCache.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT supplier_id, name, address, contact_person, contact_number, bank_name 
                                     FROM suppliers
                                     ORDER BY name";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            supplierCache.Add(new SupplierRecord
                            {
                                SupplierId = reader.GetInt32("supplier_id"),
                                Name = reader["name"]?.ToString(),
                                Address = reader["address"]?.ToString(),
                                ContactPerson = reader["contact_person"]?.ToString(),
                                ContactInfo = reader["contact_number"]?.ToString(),
                                BankName = reader["bank_name"]?.ToString()
                            });
                        }
                    }
                }

                ApplySupplierFilter(textBox1.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load suppliers: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void ApplySupplierFilter(string filterTerm)
        {
            string term = (filterTerm ?? string.Empty).Trim();
            dataGridView2.Rows.Clear();

            IEnumerable<SupplierRecord> records = supplierCache;

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.ToLowerInvariant();
                records = records.Where(r =>
                    (r.Name ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (r.Address ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (r.ContactPerson ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (r.ContactInfo ?? string.Empty).ToLowerInvariant().Contains(term) ||
                    (r.BankName ?? string.Empty).ToLowerInvariant().Contains(term));
            }

            foreach (var record in records)
            {
                int rowIndex = dataGridView2.Rows.Add(
                    record.Name,
                    record.Address,
                    record.ContactPerson,
                    record.ContactInfo,
                    record.BankName
                );

                dataGridView2.Rows[rowIndex].Tag = record.SupplierId;
            }
        }

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var row = dataGridView2.Rows[e.RowIndex];
            if (row?.Tag == null || !int.TryParse(row.Tag.ToString(), out int supplierId))
                return;

            var columnName = dataGridView2.Columns[e.ColumnIndex].Name;
            if (columnName == "botedit")
            {
                using (var updateForm = new UpdateSupplierInfo(supplierId))
                {
                    if (updateForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadSuppliers();
                    }
                }
            }
            else if (columnName == "botview")
            {
                if (loggedInUserId == 0)
                {
                    MessageBox.Show("Unable to determine current user. Please log in again.", "Authentication Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (var viewForm = new ViewSupplier(supplierId, loggedInUserId))
                {
                    viewForm.ShowDialog(this);
                }
            }
        }

        private void AddSupplierBtn_Click(object sender, EventArgs e)
        {
            if (loggedInUserId == 0)
            {
                MessageBox.Show("Unable to determine current user. Please log in again.", "Authentication Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var addSupplierForm = new AddSupplier(loggedInUserId))
            {
                if (addSupplierForm.ShowDialog(this) == DialogResult.OK)
                {
                    LoadSuppliers();
                }
            }
        }
    }
}