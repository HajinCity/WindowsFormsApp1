using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class ViewDEV : Form
    {
        private readonly int devId;
        private readonly int loggedInUserId;
        private byte[] documentBytes;
        private string storedDocumentExtension;
        private Button downloadDocumentButton;
        private Label documentStatusLabel;
        private bool isFormattingGrossAmount;
        private bool isFormattingDeductions;
        private bool isFormattingNetAmount;

        public ViewDEV()
        {
            InitializeComponent();
            MakeFieldsReadOnly();
            InitializeDocumentControls();
            InitializeAmountFormatting();
            if (ExportToCSV != null)
            {
                ExportToCSV.Click += ExportToCSV_Click;
            }
        }

        public ViewDEV(int devId, int userId) : this()
        {
            this.devId = devId;
            this.loggedInUserId = userId;
            if (devId > 0)
            {
                LoadDEVDetails();
            }
        }

        private void MakeFieldsReadOnly()
        {
            // Make all text fields read-only
            dev_no.ReadOnly = true;
            fundcluster.ReadOnly = true;
            orsbursNo.ReadOnly = true;
            payee.ReadOnly = true;
            jev_no.ReadOnly = true;
            address.ReadOnly = true;
            particulars.ReadOnly = true;
            mop.ReadOnly = true;
            respcenter.ReadOnly = true;
            tinNo.ReadOnly = true;
            mfopap.ReadOnly = true;
            taxType.ReadOnly = true;
            grossAmount.ReadOnly = true;
            deductions.ReadOnly = true;
            netAmount.ReadOnly = true;
            Status.ReadOnly = true;
            ApOfficer.ReadOnly = true;

            // Make date pickers read-only
            dev_date.Enabled = false;
            dateofJEV.Enabled = false;

            // Hide upload UI elements (read-only view)
            pictureBox1.Visible = false;
            label21.Visible = false;
            label22.Visible = false;
        }

        private void InitializeAmountFormatting()
        {
            grossAmount.TextChanged += GrossAmount_TextChanged;
            deductions.TextChanged += Deductions_TextChanged;
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

        private void LoadDEVDetails()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT dev_no, date, fund_cluster, ora_serialno, payee, jev_no,
                                            address, dateof_jev, particulars, mode_of_payment, responsibility_center,
                                            tin, mfo_pap, tax_type, gross_amount, deductions,
                                            net_amount, status, approving_officer, documents
                                     FROM dev
                                     WHERE dev_id = @devId
                                     LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@devId", devId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("DEV record could not be found.", "Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            dev_no.Text = reader["dev_no"]?.ToString();
                            fundcluster.Text = reader["fund_cluster"]?.ToString();
                            orsbursNo.Text = reader["ora_serialno"]?.ToString();
                            payee.Text = reader["payee"]?.ToString();
                            jev_no.Text = reader["jev_no"]?.ToString();
                            address.Text = reader["address"]?.ToString();
                            particulars.Text = reader["particulars"]?.ToString();
                            mop.Text = reader["mode_of_payment"]?.ToString();
                            respcenter.Text = reader["responsibility_center"]?.ToString();
                            tinNo.Text = reader["tin"]?.ToString();
                            mfopap.Text = reader["mfo_pap"]?.ToString();
                            taxType.Text = reader["tax_type"]?.ToString();
                            grossAmount.Text = reader["gross_amount"]?.ToString();
                            deductions.Text = reader["deductions"]?.ToString();
                            netAmount.Text = reader["net_amount"]?.ToString();
                            Status.Text = reader["status"]?.ToString();
                            ApOfficer.Text = reader["approving_officer"]?.ToString();

                            FormatCurrencyDisplay(grossAmount);
                            FormatCurrencyDisplay(deductions);
                            FormatCurrencyDisplay(netAmount);

                            if (reader["date"] != DBNull.Value)
                            {
                                dev_date.Value = reader.GetDateTime("date");
                            }
                            if (reader["dateof_jev"] != DBNull.Value)
                            {
                                dateofJEV.Value = reader.GetDateTime("dateof_jev");
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
                
                // Log user activity
                LogUserActivity(
                    loggedInUserId,
                    "Viewed",
                    "DEV Management",
                    $"Viewed DEV entry: {dev_no.Text}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load DEV information: {ex.Message}",
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
                // Hide upload UI elements (read-only view)
                pictureBox1.Visible = false;
                if (label21 != null) label21.Visible = false;
                if (label22 != null) label22.Visible = false;

                downloadDocumentButton.Visible = true;
                documentStatusLabel.Visible = true;
                documentStatusLabel.Text = "A document is attached to this entry.";
                downloadDocumentButton.Text = "Download / View Document";
            }
            else
            {
                // Show upload UI elements but make them non-interactive (read-only)
                pictureBox1.Visible = true;
                if (label21 != null)
                {
                    label21.Visible = true;
                    label21.Text = "No document attached";
                }
                if (label22 != null)
                {
                    label22.Visible = true;
                    label22.Text = "";
                }

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

            string baseName = string.IsNullOrWhiteSpace(dev_no.Text)
                ? "dev_document"
                : dev_no.Text.Trim().Replace(" ", "_");

            return $"{baseName}{extension}";
        }

        private void GrossAmount_TextChanged(object sender, EventArgs e)
        {
            FormatCurrencyDisplay(grossAmount);
        }

        private void Deductions_TextChanged(object sender, EventArgs e)
        {
            FormatCurrencyDisplay(deductions);
        }

        private void FormatCurrencyDisplay(TextBox textBox)
        {
            if (textBox == null || isFormattingGrossAmount || isFormattingDeductions || isFormattingNetAmount)
            {
                return;
            }

            string formatted = FormatNumberWithFraction(textBox.Text);
            if (formatted != textBox.Text)
            {
                if (textBox == grossAmount)
                {
                    isFormattingGrossAmount = true;
                    textBox.Text = formatted;
                    isFormattingGrossAmount = false;
                }
                else if (textBox == deductions)
                {
                    isFormattingDeductions = true;
                    textBox.Text = formatted;
                    isFormattingDeductions = false;
                }
                else if (textBox == netAmount)
                {
                    isFormattingNetAmount = true;
                    textBox.Text = formatted;
                    isFormattingNetAmount = false;
                }
            }
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

        private void LogUserActivity(int userId, string action, string module, string details)
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"
                        INSERT INTO userlogs (user_id, users, action, module, details, ip_address, action_timestamp)
                        SELECT 
                            u.user_id,
                            u.full_name,
                            @action,
                            @module,
                            @details,
                            @ip_address,
                            NOW()
                        FROM users u
                        WHERE u.user_id = @user_id;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        command.Parameters.AddWithValue("@action", action);
                        command.Parameters.AddWithValue("@module", module);
                        command.Parameters.AddWithValue("@details", details ?? string.Empty);
                        command.Parameters.AddWithValue("@ip_address", GetLocalIpAddress());

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't block view operation
                System.Diagnostics.Debug.WriteLine($"Failed to log user activity: {ex.Message}");
            }
        }

        private string GetLocalIpAddress()
        {
            try
            {
                string localIP = "";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }

                return string.IsNullOrEmpty(localIP) ? "Unknown" : localIP;
            }
            catch
            {
                return "Unknown";
            }
        }

        private void ExportToCSV_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export DEV Data to CSV";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                string devNumber = string.IsNullOrWhiteSpace(dev_no.Text) ? "DEV" : dev_no.Text.Trim().Replace(" ", "_");
                saveFileDialog.FileName = $"DEV_{devNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write DEV Header Information Section
                        writer.WriteLine("DEV INFORMATION");
                        writer.WriteLine("================");
                        writer.WriteLine($"DEV No.,{EscapeForCsv(dev_no.Text)}");
                        writer.WriteLine($"Date,{EscapeForCsv(dev_date.Value.ToShortDateString())}");
                        writer.WriteLine($"Fund Cluster,{EscapeForCsv(fundcluster.Text)}");
                        writer.WriteLine($"ORS/BURS Serial No.,{EscapeForCsv(orsbursNo.Text)}");
                        writer.WriteLine($"Payee,{EscapeForCsv(payee.Text)}");
                        writer.WriteLine($"JEV No.,{EscapeForCsv(jev_no.Text)}");
                        writer.WriteLine($"Address,{EscapeForCsv(address.Text)}");
                        writer.WriteLine($"Date of JEV,{EscapeForCsv(dateofJEV.Value.ToShortDateString())}");
                        writer.WriteLine($"Particulars,{EscapeForCsv(particulars.Text)}");
                        writer.WriteLine($"Mode of Payment,{EscapeForCsv(mop.Text)}");
                        writer.WriteLine($"Responsibility Center,{EscapeForCsv(respcenter.Text)}");
                        writer.WriteLine($"TIN No.,{EscapeForCsv(tinNo.Text)}");
                        writer.WriteLine($"MFO/PAP,{EscapeForCsv(mfopap.Text)}");
                        writer.WriteLine($"Tax Type,{EscapeForCsv(taxType.Text)}");
                        writer.WriteLine($"Gross Amount,{EscapeForCsv(grossAmount.Text)}");
                        writer.WriteLine($"Deductions,{EscapeForCsv(deductions.Text)}");
                        writer.WriteLine($"Net Amount,{EscapeForCsv(netAmount.Text)}");
                        writer.WriteLine($"Status,{EscapeForCsv(Status.Text)}");
                        writer.WriteLine($"Approving Officer,{EscapeForCsv(ApOfficer.Text)}");
                    }

                    MessageBox.Show("DEV data exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export DEV data: {ex.Message}", "Export Error",
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
