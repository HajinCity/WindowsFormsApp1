using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class AddDevEntry : Form
    {
        private bool isFormattingGrossAmount;
        private bool isFormattingDeductions;

        public AddDevEntry()
        {
            InitializeComponent();
            InitializeAmountsBehavior();
            createDEVbtn.Click += createDEVbtn_Click;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InitializeAmountsBehavior()
        {
            if (grossAmount != null)
            {
                grossAmount.KeyPress += NumericTextBox_KeyPress;
                grossAmount.TextChanged += GrossAmount_TextChanged;
            }

            if (deductions != null)
            {
                deductions.KeyPress += NumericTextBox_KeyPress;
                deductions.TextChanged += Deductions_TextChanged;
            }

            if (netAmount != null)
            {
                netAmount.ReadOnly = true;
                netAmount.BackColor = Color.WhiteSmoke;
            }
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar))
            {
                return;
            }

            if (e.KeyChar == '.' && sender is TextBox textBox)
            {
                string selectedText = textBox.SelectedText ?? string.Empty;
                if (!textBox.Text.Contains(".") || selectedText.Contains("."))
                {
                    return;
                }
            }

            e.Handled = true;
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

        private void createDEVbtn_Click(object sender, EventArgs e)
        {
            if (!AreInputsValid(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to save this DEV entry?",
                "Confirm Save",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SaveDevEntry();
                MessageBox.Show("Disbursement Voucher entry saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to save DEV entry: {ex.Message}",
                    "Save Error",
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

        private void SaveDevEntry()
        {
            byte[] documentBytes = GetDocumentBytes(); // currently returns null (no upload implemented)
            decimal grossAmountValue = ParseCurrencyValue(grossAmount.Text);
            decimal deductionsValue = ParseCurrencyValue(deductions.Text);
            decimal netAmountValue = ParseCurrencyValue(netAmount.Text);

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            {
                string insertDev = @"INSERT INTO dev
                    (dev_no, date, fund_cluster, ora_serialno, payee, jev_no,
                     address, dateof_jev, particulars, mode_of_payment, responsibility_center,
                     tin_no, mfo_pap, tax_type, gross_amount, deductions,
                     net_amount, status, approving_officer, documents)
                    VALUES
                    (@dev_no, @date, @fund_cluster, @ora_serialno, @payee, @jev_no,
                     @address, @dateof_jev, @particulars, @mode_of_payment, @responsibility_center,
                     @tin_no, @mfo_pap, @tax_type, @gross_amount, @deductions,
                     @net_amount, @status, @approving_officer, @documents);";

                using (MySqlCommand cmd = new MySqlCommand(insertDev, connection))
                {
                    cmd.Parameters.AddWithValue("@dev_no", dev_no.Text.Trim());
                    cmd.Parameters.AddWithValue("@date", dev_date.Value.Date);
                    cmd.Parameters.AddWithValue("@fund_cluster", fundcluster.Text.Trim());
                    cmd.Parameters.AddWithValue("@ora_serialno", orsbursNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@payee", payee.Text.Trim());
                    cmd.Parameters.AddWithValue("@jev_no", jev_no.Text.Trim());
                    cmd.Parameters.AddWithValue("@address", address.Text.Trim());
                    cmd.Parameters.AddWithValue("@dateof_jev", dateofJEV.Value.Date);
                    cmd.Parameters.AddWithValue("@particulars", particulars.Text.Trim());
                    cmd.Parameters.AddWithValue("@mode_of_payment", mop.Text.Trim());
                    cmd.Parameters.AddWithValue("@responsibility_center", respcenter.Text.Trim());
                    cmd.Parameters.AddWithValue("@tin_no", tinNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@mfo_pap", mfopap.Text.Trim());
                    cmd.Parameters.AddWithValue("@tax_type", taxType.Text.Trim());
                    cmd.Parameters.AddWithValue("@gross_amount", grossAmountValue);
                    cmd.Parameters.AddWithValue("@deductions", deductionsValue);
                    cmd.Parameters.AddWithValue("@net_amount", netAmountValue);
                    cmd.Parameters.AddWithValue("@status", Status.Text.Trim());
                    cmd.Parameters.AddWithValue("@approving_officer", ApOfficer.Text.Trim());

                    var documentParam = cmd.Parameters.Add("@documents", MySqlDbType.LongBlob);
                    documentParam.Value = (documentBytes == null || documentBytes.Length == 0)
                        ? (object)DBNull.Value
                        : documentBytes;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        private byte[] GetDocumentBytes()
        {
            // Placeholder for future file upload implementation.
            return null;
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

        private decimal GetCurrencyValueOrZero(TextBox textBox, out bool isValid)
        {
            if (textBox == null || string.IsNullOrWhiteSpace(textBox.Text))
            {
                isValid = true;
                return 0m;
            }

            if (decimal.TryParse(
                    textBox.Text.Replace(",", ""),
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                    CultureInfo.InvariantCulture,
                    out decimal value))
            {
                isValid = true;
                return value;
            }

            isValid = false;
            return 0m;
        }

        private bool TryParseCurrencyValue(string text, out decimal value)
        {
            string numericText = text?.Replace(",", "").Trim();
            if (string.IsNullOrWhiteSpace(numericText))
            {
                value = 0m;
                return false;
            }

            return decimal.TryParse(
                numericText,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out value);
        }

        private decimal ParseCurrencyValue(string text)
        {
            if (TryParseCurrencyValue(text, out decimal value))
            {
                return value;
            }

            throw new InvalidOperationException("Invalid numeric value.");
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
