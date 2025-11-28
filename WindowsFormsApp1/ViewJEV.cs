using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
    public partial class ViewJEV : Form
    {
        private readonly int jevId;
        private byte[] documentBytes;
        private string storedDocumentExtension;
        private Button downloadDocumentButton;
        private Label documentStatusLabel;

        public ViewJEV()
        {
            InitializeComponent();
            MakeFieldsReadOnly();
            InitializeDocumentControls();
            ExportToCSV.Click += ExportToCSV_Click;
        }

        public ViewJEV(int jevId) : this()
        {
            this.jevId = jevId;
            if (jevId > 0)
            {
                LoadJEVDetails();
            }
        }

        private void MakeFieldsReadOnly()
        {
            // Make all text fields read-only
            jev_no.ReadOnly = true;
            rspCode.ReadOnly = true;
            uacscode.ReadOnly = true;
            account.ReadOnly = true;
            particulars.ReadOnly = true;
            taxtype.ReadOnly = true;
            grossAmount.ReadOnly = true;
            deductions.ReadOnly = true;
            netAmount.ReadOnly = true;
            status.ReadOnly = true;
            approvingOfficer.ReadOnly = true;

            // Make date picker read-only
            jevDate.Enabled = false;

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

        private void LoadJEVDetails()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT jev_no, date, responsibility_center, uacs_code, account, particulars,
                                            gross_amount, deductions, tax_type, net_amount, status, approving_officer, documents
                                     FROM jev
                                     WHERE jev_id = @jevId
                                     LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@jevId", jevId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("JEV record could not be found.", "Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            jev_no.Text = reader["jev_no"]?.ToString();
                            rspCode.Text = reader["responsibility_center"]?.ToString();
                            uacscode.Text = reader["uacs_code"]?.ToString();
                            account.Text = reader["account"]?.ToString();
                            particulars.Text = reader["particulars"]?.ToString();
                            taxtype.Text = reader["tax_type"]?.ToString();
                            grossAmount.Text = reader["gross_amount"]?.ToString();
                            deductions.Text = reader["deductions"]?.ToString();
                            netAmount.Text = reader["net_amount"]?.ToString();
                            status.Text = reader["status"]?.ToString();
                            approvingOfficer.Text = reader["approving_officer"]?.ToString();

                            FormatCurrencyDisplay(grossAmount);
                            FormatCurrencyDisplay(deductions);
                            FormatCurrencyDisplay(netAmount);

                            if (reader["date"] != DBNull.Value)
                            {
                                jevDate.Value = reader.GetDateTime("date");
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
                    $"Unable to load JEV information: {ex.Message}",
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

            string baseName = string.IsNullOrWhiteSpace(jev_no.Text)
                ? "jev_document"
                : jev_no.Text.Trim().Replace(" ", "_");

            return $"{baseName}{extension}";
        }

        private void ExportToCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export JEV Data to CSV";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                string jevNumber = string.IsNullOrWhiteSpace(jev_no.Text) ? "JEV" : jev_no.Text.Trim().Replace(" ", "_");
                saveFileDialog.FileName = $"JEV_{jevNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write JEV Header Information Section
                        writer.WriteLine("JEV INFORMATION");
                        writer.WriteLine("===============");
                        writer.WriteLine($"JEV No.,{EscapeForCsv(jev_no.Text)}");
                        writer.WriteLine($"Date,{EscapeForCsv(jevDate.Value.ToShortDateString())}");
                        writer.WriteLine($"Responsibility Center Code,{EscapeForCsv(rspCode.Text)}");
                        writer.WriteLine($"UACS Code,{EscapeForCsv(uacscode.Text)}");
                        writer.WriteLine($"Account,{EscapeForCsv(account.Text)}");
                        writer.WriteLine($"Particulars,{EscapeForCsv(particulars.Text)}");
                        writer.WriteLine($"Tax Type,{EscapeForCsv(taxtype.Text)}");
                        writer.WriteLine($"Gross Amount,{EscapeForCsv(grossAmount.Text)}");
                        writer.WriteLine($"Deductions,{EscapeForCsv(deductions.Text)}");
                        writer.WriteLine($"Net Amount,{EscapeForCsv(netAmount.Text)}");
                        writer.WriteLine($"Status,{EscapeForCsv(status.Text)}");
                        writer.WriteLine($"Approving Officer,{EscapeForCsv(approvingOfficer.Text)}");
                    }

                    MessageBox.Show("JEV data exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export JEV data: {ex.Message}", "Export Error",
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

        private void FormatCurrencyDisplay(TextBox textBox)
        {
            if (textBox == null)
            {
                return;
            }

            string formatted = FormatNumberWithFraction(textBox.Text);
            textBox.Text = formatted;
        }

        private string FormatNumberWithFraction(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            string numericText = input.Replace(",", "").Trim();
            if (numericText.Length == 0)
            {
                return string.Empty;
            }

            bool isNegative = numericText.StartsWith("-");
            if (isNegative)
            {
                numericText = numericText.Substring(1);
            }

            int decimalIndex = numericText.IndexOf('.');
            string integerPart = decimalIndex >= 0 ? numericText.Substring(0, decimalIndex) : numericText;
            string fractionalPart = decimalIndex >= 0 ? numericText.Substring(decimalIndex) : string.Empty;

            if (integerPart.Length == 0)
            {
                integerPart = "0";
            }

            if (!decimal.TryParse(
                integerPart,
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out decimal integerValue))
            {
                return input;
            }

            string formattedInteger = string.Format(CultureInfo.InvariantCulture, "{0:N0}", integerValue);
            if (isNegative && !formattedInteger.StartsWith("-", StringComparison.Ordinal))
            {
                formattedInteger = "-" + formattedInteger;
            }

            return formattedInteger + fractionalPart;
        }
    }
}
