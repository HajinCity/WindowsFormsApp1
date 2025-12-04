using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class UpdateDEV : Form
    {
        private readonly int devId;
        private readonly int loggedInUserId;
        private byte[] documentBytes;
        private byte[] originalDocumentBytes;
        private string storedDocumentExtension;
        private bool isFormattingGrossAmount;
        private bool isFormattingDeductions;
        private bool isFormattingNetAmount;

        public UpdateDEV()
        {
            InitializeComponent();
            InitializeAmountFormatting();
            UpdateDevbtn.Click += UpdateDevbtn_Click;
            cancel.Click += cancel_Click;
        }

        public UpdateDEV(int devId, int userId = 0) : this()
        {
            this.devId = devId;
            this.loggedInUserId = userId;
            if (devId > 0)
            {
                LoadDEVDetails();
            }
        }

        private void InitializeAmountFormatting()
        {
            grossAmount.TextChanged += GrossAmount_TextChanged;
            deductions.TextChanged += Deductions_TextChanged;
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
                            originalDocumentBytes = documentBytes;
                            storedDocumentExtension = GuessFileExtension(documentBytes);
                        }
                    }
                }
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

        private void UpdateDevbtn_Click(object sender, EventArgs e)
        {
            if (!AreInputsValid(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to update this DEV entry?",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                UpdateDEVEntry();
                MessageBox.Show("DEV entry updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update DEV entry: {ex.Message}",
                    "Update Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool AreInputsValid(out string message)
        {
            var requiredFields = new List<(string Value, string Label)>
            {
                (dev_no.Text, "DEV No."),
                (fundcluster.Text, "Fund Cluster"),
                (orsbursNo.Text, "ORS/BURS Serial No."),
                (payee.Text, "Payee"),
                (jev_no.Text, "JEV No."),
                (address.Text, "Address"),
                (particulars.Text, "Particulars"),
                (mop.Text, "Mode of Payment"),
                (respcenter.Text, "Responsibility Center"),
                (tinNo.Text, "TIN No."),
                (mfopap.Text, "MFO/PAP"),
                (taxType.Text, "Tax Type"),
                (grossAmount.Text, "Gross Amount"),
                (deductions.Text, "Deductions"),
                (netAmount.Text, "Net Amount"),
                (Status.Text, "Status"),
                (ApOfficer.Text, "Approving Officer")
            };

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    message = $"{field.Label} is required and cannot be empty.";
                    return false;
                }
            }

            if (!TryParseCurrencyValue(grossAmount.Text, out _))
            {
                message = "Gross Amount must be a valid number.";
                return false;
            }

            if (!TryParseCurrencyValue(deductions.Text, out _))
            {
                message = "Deductions must be a valid number.";
                return false;
            }

            if (!TryParseCurrencyValue(netAmount.Text, out _))
            {
                message = "Net Amount must be a valid number.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private bool TryParseCurrencyValue(string text, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string numericText = text.Replace(",", "").Trim();
            return decimal.TryParse(numericText, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private decimal ParseCurrencyValue(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            string numericText = text.Replace(",", "").Trim();
            if (decimal.TryParse(numericText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
            {
                return value;
            }

            return 0;
        }

        private void UpdateDEVEntry()
        {
            byte[] documentBytesToSave = documentBytes ?? originalDocumentBytes;
            decimal grossAmountValue = ParseCurrencyValue(grossAmount.Text);
            decimal deductionsValue = ParseCurrencyValue(deductions.Text);
            decimal netAmountValue = ParseCurrencyValue(netAmount.Text);

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            {
                string updateQuery = @"UPDATE dev SET
                                        date = @date,
                                        fund_cluster = @fund_cluster,
                                        ora_serialno = @ora_serialno,
                                        payee = @payee,
                                        jev_no = @jev_no,
                                        address = @address,
                                        dateof_jev = @dateof_jev,
                                        particulars = @particulars,
                                        mode_of_payment = @mode_of_payment,
                                        responsibility_center = @responsibility_center,
                                        tin = @tin,
                                        mfo_pap = @mfo_pap,
                                        tax_type = @tax_type,
                                        gross_amount = @gross_amount,
                                        deductions = @deductions,
                                        net_amount = @net_amount,
                                        status = @status,
                                        approving_officer = @approving_officer,
                                        documents = @documents
                                      WHERE dev_id = @dev_id";

                using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@dev_id", devId);
                    command.Parameters.AddWithValue("@date", dev_date.Value.Date);
                    command.Parameters.AddWithValue("@fund_cluster", fundcluster.Text.Trim());
                    command.Parameters.AddWithValue("@ora_serialno", orsbursNo.Text.Trim());
                    command.Parameters.AddWithValue("@payee", payee.Text.Trim());
                    command.Parameters.AddWithValue("@jev_no", jev_no.Text.Trim());
                    command.Parameters.AddWithValue("@address", address.Text.Trim());
                    command.Parameters.AddWithValue("@dateof_jev", dateofJEV.Value.Date);
                    command.Parameters.AddWithValue("@particulars", particulars.Text.Trim());
                    command.Parameters.AddWithValue("@mode_of_payment", mop.Text.Trim());
                    command.Parameters.AddWithValue("@responsibility_center", respcenter.Text.Trim());
                    command.Parameters.AddWithValue("@tin", tinNo.Text.Trim());
                    command.Parameters.AddWithValue("@mfo_pap", mfopap.Text.Trim());
                    command.Parameters.AddWithValue("@tax_type", taxType.Text.Trim());
                    command.Parameters.AddWithValue("@gross_amount", grossAmountValue);
                    command.Parameters.AddWithValue("@deductions", deductionsValue);
                    command.Parameters.AddWithValue("@net_amount", netAmountValue);
                    command.Parameters.AddWithValue("@status", Status.Text.Trim());
                    command.Parameters.AddWithValue("@approving_officer", ApOfficer.Text.Trim());

                    var documentParam = command.Parameters.Add("@documents", MySqlDbType.LongBlob);
                    documentParam.Value = (documentBytesToSave == null || documentBytesToSave.Length == 0)
                        ? (object)DBNull.Value
                        : documentBytesToSave;

                    command.ExecuteNonQuery();
                }

                // Log user activity
                if (loggedInUserId > 0)
                {
                    LogUserActivity(
                        loggedInUserId,
                        "Updated",
                        "DEV Management",
                        $"Updated DEV entry: {dev_no.Text}");
                }
            }
        }

        private void GrossAmount_TextChanged(object sender, EventArgs e)
        {
            FormatCurrencyTextBox(grossAmount, ref isFormattingGrossAmount);
            UpdateNetAmount();
        }

        private void Deductions_TextChanged(object sender, EventArgs e)
        {
            FormatCurrencyTextBox(deductions, ref isFormattingDeductions);
            UpdateNetAmount();
        }

        private void UpdateNetAmount()
        {
            decimal gross = GetCurrencyValueOrZero(grossAmount, out bool grossValid);
            decimal deduct = GetCurrencyValueOrZero(deductions, out bool deductValid);

            if (grossValid && deductValid)
            {
                if (string.IsNullOrWhiteSpace(grossAmount.Text) && string.IsNullOrWhiteSpace(deductions.Text))
                {
                    netAmount.Text = string.Empty;
                }
                else
                {
                    decimal net = gross - deduct;
                    netAmount.Text = FormatNumberWithFraction(net.ToString("0.##", CultureInfo.InvariantCulture));
                }
            }
            else
            {
                netAmount.Text = string.Empty;
            }
        }

        private decimal GetCurrencyValueOrZero(TextBox textBox, out bool isValid)
        {
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text))
            {
                isValid = true;
                return 0;
            }

            string numericText = textBox.Text.Replace(",", "").Trim();
            isValid = decimal.TryParse(numericText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value);
            return isValid ? value : 0;
        }

        private void FormatCurrencyTextBox(TextBox textBox, ref bool isFormattingFlag)
        {
            if (isFormattingFlag || textBox == null)
            {
                return;
            }

            string currentText = textBox.Text;
            if (string.IsNullOrWhiteSpace(currentText))
            {
                return;
            }

            string formattedText = FormatNumberWithFraction(currentText);
            if (formattedText == currentText)
            {
                return;
            }

            isFormattingFlag = true;
            int selectionFromEnd = currentText.Length - textBox.SelectionStart;
            textBox.Text = formattedText;
            int newSelectionStart = formattedText.Length - selectionFromEnd;
            if (newSelectionStart < 0)
            {
                newSelectionStart = 0;
            }
            textBox.SelectionStart = Math.Min(newSelectionStart, textBox.Text.Length);
            textBox.SelectionLength = 0;
            isFormattingFlag = false;
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
                // Log error but don't block update operation
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

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
