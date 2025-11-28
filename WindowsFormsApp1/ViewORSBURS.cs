using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
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
    public partial class ViewORSBURS : Form
    {
        private readonly int orsBursId;
        private byte[] documentBytes;
        private string storedDocumentExtension;
        private Button downloadDocumentButton;
        private Label documentStatusLabel;
        private bool isFormattingAmountText;

        public ViewORSBURS()
        {
            InitializeComponent();
            MakeFieldsReadOnly();
            InitializeDocumentControls();
            ExportToCSV.Click += ExportToCSV_Click;
            amount.TextChanged += Amount_TextChanged;
        }

        public ViewORSBURS(int orsBursId) : this()
        {
            this.orsBursId = orsBursId;
            if (orsBursId > 0)
            {
                LoadOrsBursDetails();
            }
        }

        private void MakeFieldsReadOnly()
        {
            // Make all text fields read-only
            serialNo.ReadOnly = true;
            textBox1.ReadOnly = true;
            Payee.ReadOnly = true;
            Office.ReadOnly = true;
            FundCluster.ReadOnly = true;
            Address.ReadOnly = true;
            ResponsibilityCenter.ReadOnly = true;
            Particulars.ReadOnly = true;
            MFOPAP.ReadOnly = true;
            uacscode.ReadOnly = true;
            amount.ReadOnly = true;
            approvingOfficer.ReadOnly = true;
            remarks.ReadOnly = true;

            // Make date picker read-only
            date.Enabled = false;

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

        private void LoadOrsBursDetails()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT ora_serialno, date, fund_cluster, po_no, payee, office, address,
                                            responsibility_center, particulars, mfo_pap, uacs_oc,
                                            amount, approving_officer, remarks, documents
                                     FROM ora_burono
                                     WHERE ora_burono = @orsBursId
                                     LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@orsBursId", orsBursId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("ORS-BURS record could not be found.", "Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            serialNo.Text = reader["ora_serialno"]?.ToString();
                            textBox1.Text = reader["po_no"]?.ToString();
                            Payee.Text = reader["payee"]?.ToString();
                            Office.Text = reader["office"]?.ToString();
                            FundCluster.Text = reader["fund_cluster"]?.ToString();
                            Address.Text = reader["address"]?.ToString();
                            ResponsibilityCenter.Text = reader["responsibility_center"]?.ToString();
                            Particulars.Text = reader["particulars"]?.ToString();
                            MFOPAP.Text = reader["mfo_pap"]?.ToString();
                            uacscode.Text = reader["uacs_oc"]?.ToString();
                            amount.Text = reader["amount"]?.ToString();
                            approvingOfficer.Text = reader["approving_officer"]?.ToString();
                            remarks.Text = reader["remarks"]?.ToString();

                            if (reader["date"] != DBNull.Value)
                            {
                                date.Value = reader.GetDateTime("date");
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
                }

                UpdateDocumentSection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load ORS-BURS information: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateDocumentSection()
        {
            bool hasDocument = documentBytes != null && documentBytes.Length > 0;

            if (hasDocument)
            {
                downloadDocumentButton.Visible = true;
                documentStatusLabel.Visible = true;
                documentStatusLabel.Text = "A document is attached to this entry.";
                downloadDocumentButton.Text = "Download / View Document";
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

            string baseName = string.IsNullOrWhiteSpace(serialNo.Text)
                ? "ors_burs_document"
                : serialNo.Text.Trim().Replace(" ", "_");

            return $"{baseName}{extension}";
        }

        private void ExportToCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export ORS-BURS Data to CSV";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                string serialNumber = string.IsNullOrWhiteSpace(serialNo.Text) ? "ORSBURS" : serialNo.Text.Trim().Replace(" ", "_");
                saveFileDialog.FileName = $"ORSBURS_{serialNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write ORS-BURS Header Information Section
                        writer.WriteLine("ORS-BURS INFORMATION");
                        writer.WriteLine("=====================");
                        writer.WriteLine($"Serial Number,{EscapeForCsv(serialNo.Text)}");
                        writer.WriteLine($"Date,{EscapeForCsv(date.Value.ToShortDateString())}");
                        writer.WriteLine($"Fund Cluster,{EscapeForCsv(FundCluster.Text)}");
                        writer.WriteLine($"PO Number,{EscapeForCsv(textBox1.Text)}");
                        writer.WriteLine($"Payee,{EscapeForCsv(Payee.Text)}");
                        writer.WriteLine($"Office,{EscapeForCsv(Office.Text)}");
                        writer.WriteLine($"Address,{EscapeForCsv(Address.Text)}");
                        writer.WriteLine($"Responsibility Center,{EscapeForCsv(ResponsibilityCenter.Text)}");
                        writer.WriteLine($"Particulars,{EscapeForCsv(Particulars.Text)}");
                        writer.WriteLine($"MFO/PAP,{EscapeForCsv(MFOPAP.Text)}");
                        writer.WriteLine($"UACS Code,{EscapeForCsv(uacscode.Text)}");
                        writer.WriteLine($"Amount,{EscapeForCsv(amount.Text)}");
                        writer.WriteLine($"Approving Officer,{EscapeForCsv(approvingOfficer.Text)}");
                        writer.WriteLine($"Remarks,{EscapeForCsv(remarks.Text)}");
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

        private void Amount_TextChanged(object sender, EventArgs e)
        {
            if (isFormattingAmountText)
            {
                return;
            }

            string currentText = amount.Text;
            if (string.IsNullOrWhiteSpace(currentText))
            {
                return;
            }

            string cleanText = currentText.Replace(",", "");
            if (!decimal.TryParse(cleanText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal parsedValue))
            {
                return;
            }

            string formattedInteger = string.Format(
                CultureInfo.InvariantCulture,
                "{0:N0}",
                Math.Truncate(parsedValue));

            int decimalIndex = cleanText.IndexOf('.');
            string fractionalPart = decimalIndex >= 0 ? cleanText.Substring(decimalIndex) : string.Empty;
            string formattedText = formattedInteger + fractionalPart;

            isFormattingAmountText = true;
            amount.Text = formattedText;
            amount.SelectionStart = amount.Text.Length;
            amount.SelectionLength = 0;
            isFormattingAmountText = false;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
