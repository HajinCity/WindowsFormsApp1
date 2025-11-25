using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class SupplierForm : Form
    {
        public SupplierForm()
        {
            InitializeComponent();
            addSupplierBtn.Click += AddSupplierBtn_Click;
            this.Load += SupplierForm_Load;
            dataGridView1.CellContentClick += DataGridView1_CellContentClick;
            ConfigureActionColumns();
        }

        private void SupplierForm_Load(object sender, EventArgs e)
        {
            LoadSuppliers();
        }

        private void ConfigureActionColumns()
        {
            if (BtnEdit != null)
            {
                BtnEdit.Image = Properties.Resources.Edit;
                BtnEdit.ImageLayout = DataGridViewImageCellLayout.Zoom;
            }

            if (BtnView != null)
            {
                BtnView.Image = Properties.Resources.Document;
                BtnView.ImageLayout = DataGridViewImageCellLayout.Zoom;
            }
        }

        private void LoadSuppliers()
        {
            try
            {
                dataGridView1.Rows.Clear();

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
                            int supplierId = reader.GetInt32("supplier_id");
                            string name = reader["name"]?.ToString();
                            string address = reader["address"]?.ToString();
                            string contactPerson = reader["contact_person"]?.ToString();
                            string contactInfo = reader["contact_number"]?.ToString();
                            string bankName = reader["bank_name"]?.ToString();

                            int rowIndex = dataGridView1.Rows.Add(
                                name,
                                address,
                                contactPerson,
                                contactInfo,
                                bankName,
                                Properties.Resources.Edit,
                                Properties.Resources.Document
                            );

                            dataGridView1.Rows[rowIndex].Tag = supplierId;
                        }
                    }
                }
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

        private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var row = dataGridView1.Rows[e.RowIndex];
            if (row?.Tag == null || !int.TryParse(row.Tag.ToString(), out int supplierId))
                return;

            var columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            if (columnName == nameof(BtnEdit))
            {
                using (var updateForm = new UpdateSupplierInfo(supplierId))
                {
                    if (updateForm.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadSuppliers();
                    }
                }
            }
            else if (columnName == nameof(BtnView))
            {
                using (var viewForm = new ViewSupplier(supplierId))
                {
                    viewForm.ShowDialog(this);
                }
            }
        }

        private void AddSupplierBtn_Click(object sender, EventArgs e)
        {
            using (var addSupplierForm = new AddSupplier())
            {
                if (addSupplierForm.ShowDialog(this) == DialogResult.OK)
                {
                    LoadSuppliers();
                }
            }
        }
    }
}
