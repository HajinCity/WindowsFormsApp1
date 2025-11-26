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
        }

        private void JEVForm_Load(object sender, EventArgs e)
        {
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
        }
    }
}
