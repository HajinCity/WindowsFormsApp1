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

        public GeneralJournal()
        {
            InitializeComponent();
            addEntryBtn.Click += AddEntryBtn_Click;
            textBox1.TextChanged += TextBox1_TextChanged;
            dateTimePicker1.ValueChanged += DateFilter_ValueChanged;
            dateTimePicker2.ValueChanged += DateFilter_ValueChanged;
            ExportToCSV.Click += ExportToCSV_Click;
            this.Load += GeneralJournal_Load;
        }

        private void GeneralJournal_Load(object sender, EventArgs e)
        {
            LoadJournalEntries();
        }

        private void AddEntryBtn_Click(object sender, EventArgs e)
        {
            using (var addEntryForm = new AddNewJournalEntry())
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

                ApplyJournalFilter();
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

        private void DateFilter_ValueChanged(object sender, EventArgs e)
        {
            if (dateTimePicker1.Value.Date > dateTimePicker2.Value.Date)
            {
                dateTimePicker2.Value = dateTimePicker1.Value.Date;
            }
            ApplyJournalFilter();
        }

        private void ApplyJournalFilter()
        {
            string term = (textBox1.Text ?? string.Empty).Trim().ToLowerInvariant();
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;

            var filtered = journalCache.Where(entry =>
                entry.Date.Date >= startDate &&
                entry.Date.Date <= endDate &&
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
                    entry.Amount);
                dataGridView1.Rows[rowIndex].Tag = entry.Id;
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
    }
}
