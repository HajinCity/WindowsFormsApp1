using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class ViewIAR : Form
    {
        private readonly int iarId;
        private byte[] documentBytes;
        private string storedDocumentExtension;
        private Button downloadDocumentButton;
        private Label documentStatusLabel;

        public ViewIAR()
        {
            InitializeComponent();
            MakeFieldsReadOnly();
            InitializeDocumentControls();
            ExportToCSV.Click += ExportToCSV_Click;
        }

        public ViewIAR(int iarId) : this()
        {
            this.iarId = iarId;
            if (iarId > 0)
            {
                LoadIarDetails();
            }
        }

        private void MakeFieldsReadOnly()
        {
            // Make all text fields read-only
            iarNo.ReadOnly = true;
            supplierName.ReadOnly = true;
            RequisitioningOffice.ReadOnly = true;
            FundCluster.ReadOnly = true;
            RequistioningCnterCode.ReadOnly = true;
            po_number.ReadOnly = true;
            InvoiceNumber.ReadOnly = true;
            InspectorOfficer.ReadOnly = true;
            PropertyCustodianOfficer.ReadOnly = true;
            ReceivedStatus.ReadOnly = true;
            TotalAmount.ReadOnly = true;
            textBox11.ReadOnly = true;

            // Make date pickers read-only
            iar_date.Enabled = false;
            po_date.Enabled = false;
            date_inspected.Enabled = false;
            date_received.Enabled = false;

            // Make dataGridView1 read-only
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;

            // Hide upload UI elements (read-only view)
            pictureBox1.Visible = false;
            label18.Visible = false;
            label19.Visible = false;
        }

        private void InitializeDocumentControls()
        {
            downloadDocumentButton = new Button
            {
                Text = "Download / View Document",
                AutoSize = false,
                Size = new Size(220, 40),
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            downloadDocumentButton.FlatAppearance.BorderSize = 0;
            downloadDocumentButton.Cursor = Cursors.Hand;
            downloadDocumentButton.Click += DownloadDocumentButton_Click;
            panel1.Controls.Add(downloadDocumentButton);

            documentStatusLabel = new Label
            {
                Text = "No document attached to this entry.",
                AutoSize = true,
                ForeColor = Color.FromArgb(64, 64, 64),
                Visible = false
            };
            panel1.Controls.Add(documentStatusLabel);

            panel1.Resize += Panel1_Resize;
            PositionDocumentControls();
        }

        private void Panel1_Resize(object sender, EventArgs e)
        {
            PositionDocumentControls();
        }

        private void PositionDocumentControls()
        {
            int centerX = Math.Max(0, (panel1.Width - 220) / 2);

            if (downloadDocumentButton != null)
            {
                downloadDocumentButton.Location = new Point(centerX, 20);
            }

            if (documentStatusLabel != null)
            {
                documentStatusLabel.Location = new Point(
                    Math.Max(0, (panel1.Width - documentStatusLabel.Width) / 2),
                    70);
            }
        }

        private void DownloadDocumentButton_Click(object sender, EventArgs e)
        {
            if (documentBytes == null || documentBytes.Length == 0)
            {
                MessageBox.Show("No document is available for this entry.", "No Document",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save Supporting Document";
                string suggestedName = GetSuggestedDocumentFileName();
                string extension = Path.GetExtension(suggestedName);
                saveFileDialog.Filter = $"Document (*{extension})|*{extension}|All Files|*.*";
                saveFileDialog.DefaultExt = extension.TrimStart('.');
                saveFileDialog.FileName = suggestedName;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, documentBytes);
                        var openPrompt = MessageBox.Show(
                            "Document saved successfully. Do you want to open it now?",
                            "Document Saved",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (openPrompt == DialogResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = saveFileDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            $"Unable to save the document: {ex.Message}",
                            "Save Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadIarDetails()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT iar_no, date_iar, fund_cluster, supplier, po_no, po_date,
                                            requisitioning_office, responsibility_centercode, `Invoice No` AS invoice_no,
                                            date_inspected, inspector_offcer, date_received,
                                            property_custodian_officer, record_status, total_amount,
                                            remarks, documents
                                     FROM inspection_acceptance_report
                                     WHERE iar_id = @iarId
                                     LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@iarId", iarId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("IAR record could not be found.", "Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            iarNo.Text = reader["iar_no"]?.ToString();
                            supplierName.Text = reader["supplier"]?.ToString();
                            RequisitioningOffice.Text = reader["requisitioning_office"]?.ToString();
                            FundCluster.Text = reader["fund_cluster"]?.ToString();
                            RequistioningCnterCode.Text = reader["responsibility_centercode"]?.ToString();
                            po_number.Text = reader["po_no"]?.ToString();
                            InvoiceNumber.Text = reader["invoice_no"]?.ToString();
                            InspectorOfficer.Text = reader["inspector_offcer"]?.ToString();
                            PropertyCustodianOfficer.Text = reader["property_custodian_officer"]?.ToString();
                            ReceivedStatus.Text = reader["record_status"]?.ToString();
                            TotalAmount.Text = reader["total_amount"]?.ToString();
                            textBox11.Text = reader["remarks"]?.ToString();

                            if (reader["date_iar"] != DBNull.Value)
                            {
                                iar_date.Value = reader.GetDateTime("date_iar");
                            }
                            if (reader["po_date"] != DBNull.Value)
                            {
                                po_date.Value = reader.GetDateTime("po_date");
                            }
                            if (reader["date_inspected"] != DBNull.Value)
                            {
                                date_inspected.Value = reader.GetDateTime("date_inspected");
                            }
                            if (reader["date_received"] != DBNull.Value)
                            {
                                date_received.Value = reader.GetDateTime("date_received");
                            }

                            if (reader["documents"] != DBNull.Value)
                            {
                                documentBytes = (byte[])reader["documents"];
                            }
                            else
                            {
                                documentBytes = null;
                            }
                            storedDocumentExtension = GuessFileExtension(documentBytes);
                        }
                    }

                    LoadStockItems(connection);
                }

                UpdateDocumentSection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load IAR information: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void LoadStockItems(MySqlConnection connection)
        {
            dataGridView1.Rows.Clear();

            string query = @"SELECT stck_no, description, unit, quantity
                             FROM stock_property
                             WHERE iar_id = @iarId
                             ORDER BY stck_id";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@iarId", iarId);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dataGridView1.Rows.Add(
                            reader["stck_no"]?.ToString(),
                            reader["description"]?.ToString(),
                            reader["unit"]?.ToString(),
                            reader["quantity"]?.ToString());
                    }
                }
            }
        }

        private void UpdateDocumentSection()
        {
            bool hasDocument = documentBytes != null && documentBytes.Length > 0;

            if (hasDocument)
            {
                downloadDocumentButton.Visible = true;
                documentStatusLabel.Visible = true;
                documentStatusLabel.Text = "Document available. Click the button below to download/view.";
            }
            else
            {
                downloadDocumentButton.Visible = false;
                documentStatusLabel.Visible = true;
                documentStatusLabel.Text = "No document attached to this entry.";
            }

            PositionDocumentControls();
        }

        private string GuessFileExtension(byte[] data)
        {
            if (data == null || data.Length < 4)
            {
                return ".bin";
            }

            if (data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46)
                return ".pdf";
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                return ".png";
            if (data[0] == 0xFF && data[1] == 0xD8)
                return ".jpg";
            if (data[0] == 0x50 && data[1] == 0x4B && data[2] == 0x03 && data[3] == 0x04)
                return ".docx";
            if (data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0)
                return ".doc";

            return ".bin";
        }

        private string GetSuggestedDocumentFileName()
        {
            string extension = storedDocumentExtension;
            if (string.IsNullOrEmpty(extension))
            {
                extension = GuessFileExtension(documentBytes);
            }
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".bin";
            }
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }

            string baseName = string.IsNullOrWhiteSpace(iarNo.Text)
                ? "iar_document"
                : iarNo.Text.Trim().Replace(" ", "_");

            return $"{baseName}{extension}";
        }

        private void ExportToCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export IAR Data to CSV";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                string iarNumber = string.IsNullOrWhiteSpace(iarNo.Text) ? "IAR" : iarNo.Text.Trim().Replace(" ", "_");
                saveFileDialog.FileName = $"IAR_{iarNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write IAR Header Information Section
                        writer.WriteLine("IAR INFORMATION");
                        writer.WriteLine("================");
                        writer.WriteLine($"IAR Number,{EscapeForCsv(iarNo.Text)}");
                        writer.WriteLine($"IAR Date,{EscapeForCsv(iar_date.Value.ToShortDateString())}");
                        writer.WriteLine($"Fund Cluster,{EscapeForCsv(FundCluster.Text)}");
                        writer.WriteLine($"Supplier Name,{EscapeForCsv(supplierName.Text)}");
                        writer.WriteLine($"PO Number,{EscapeForCsv(po_number.Text)}");
                        writer.WriteLine($"PO Date,{EscapeForCsv(po_date.Value.ToShortDateString())}");
                        writer.WriteLine($"Requisitioning Office,{EscapeForCsv(RequisitioningOffice.Text)}");
                        writer.WriteLine($"Requisitioning Center Code,{EscapeForCsv(RequistioningCnterCode.Text)}");
                        writer.WriteLine($"Invoice Number,{EscapeForCsv(InvoiceNumber.Text)}");
                        writer.WriteLine($"Date Inspected,{EscapeForCsv(date_inspected.Value.ToShortDateString())}");
                        writer.WriteLine($"Inspector Officer,{EscapeForCsv(InspectorOfficer.Text)}");
                        writer.WriteLine($"Date Received,{EscapeForCsv(date_received.Value.ToShortDateString())}");
                        writer.WriteLine($"Property Custodian Officer,{EscapeForCsv(PropertyCustodianOfficer.Text)}");
                        writer.WriteLine($"Received Status,{EscapeForCsv(ReceivedStatus.Text)}");
                        writer.WriteLine($"Total Amount,{EscapeForCsv(TotalAmount.Text)}");
                        writer.WriteLine($"Remarks,{EscapeForCsv(textBox11.Text)}");
                        writer.WriteLine(); // Empty line separator

                        // Write Stock Items Section
                        writer.WriteLine("STOCK ITEMS");
                        writer.WriteLine("===========");
                        writer.WriteLine("Stock Number,Description,Unit,Quantity");

                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string stockNo = EscapeForCsv(row.Cells[0].Value?.ToString());
                            string description = EscapeForCsv(row.Cells[1].Value?.ToString());
                            string unit = EscapeForCsv(row.Cells[2].Value?.ToString());
                            string quantity = EscapeForCsv(row.Cells[3].Value?.ToString());

                            writer.WriteLine($"{stockNo},{description},{unit},{quantity}");
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

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
