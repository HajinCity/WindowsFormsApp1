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
    public partial class OrsBursForm : Form
    {
        private class OrsBursRecord
        {
            public int Id { get; set; }
            public string SerialNo { get; set; }
            public DateTime Date { get; set; }
            public string FundCluster { get; set; }
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
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            pictureBox2.Click += PictureBox2_Click;
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
                    string query = @"SELECT ora_burono, serial_no, date, fund_cluster, payee, office, 
                                     responsibility_center, approving_officer, amount
                                     FROM ora_burono
                                     ORDER BY date DESC, serial_no";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orsBursCache.Add(new OrsBursRecord
                            {
                                Id = reader.GetInt32("ora_burono"),
                                SerialNo = reader["serial_no"]?.ToString(),
                                Date = reader["date"] == DBNull.Value ? DateTime.MinValue : reader.GetDateTime("date"),
                                FundCluster = reader["fund_cluster"]?.ToString(),
                                Payee = reader["payee"]?.ToString(),
                                Office = reader["office"]?.ToString(),
                                ResponsibilityCenter = reader["responsibility_center"]?.ToString(),
                                ApprovingOfficer = reader["approving_officer"]?.ToString(),
                                Amount = reader["amount"]?.ToString(),
                                Status = "" // Status column doesn't exist in table, leaving empty for now
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
            dataGridView1.Rows.Clear();
            foreach (var entry in orsBursCache)
            {
                int rowIndex = dataGridView1.Rows.Add(
                    entry.SerialNo,
                    entry.Date == DateTime.MinValue ? "" : entry.Date.ToShortDateString(),
                    entry.FundCluster,
                    entry.Payee,
                    entry.Office,
                    entry.ResponsibilityCenter,
                    entry.ApprovingOfficer,
                    entry.Amount,
                    entry.Status);
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
            if (row?.Tag == null || !int.TryParse(row.Tag.ToString(), out int orsBursId))
            {
                return;
            }

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName == "BtnEdit")
            {
                using (var updateForm = new UpdateORSBURSForm(orsBursId))
                {
                    if (updateForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadOrsBursData();
                    }
                }
            }
            else if (columnName == "BtnView")
            {
                using (var viewForm = new ViewORSBURS(orsBursId))
                {
                    viewForm.ShowDialog(this);
                }
            }
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            // Refresh and display all data from ora_burono table
            dateTimePicker1.Value = DateTime.Today.AddYears(-10);
            dateTimePicker2.Value = DateTime.Today.AddYears(1);
            textBox1.Clear();
            LoadOrsBursData();
        }
    }
}
