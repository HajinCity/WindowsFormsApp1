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

            // Validate negative values
            if (!ValidateNoNegativeValues(out string negativeValueMessage))
            {
                MessageBox.Show(negativeValueMessage, "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate JEV exists
            if (!ValidateJEVExists(jev_no.Text.Trim(), out string jevErrorMessage))
            {
                MessageBox.Show(jevErrorMessage, "Invalid JEV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate JEV gross amount matches
            decimal grossAmountValue = ParseCurrencyValue(grossAmount.Text);
            if (!ValidateJEVGrossAmount(jev_no.Text.Trim(), grossAmountValue, out string grossAmountErrorMessage))
            {
                MessageBox.Show(grossAmountErrorMessage, "Invalid Gross Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate ORA Serial No exists
            if (!ValidateORASerialNoExists(orsbursNo.Text.Trim(), out string oraErrorMessage))
            {
                MessageBox.Show(
                    "PO number does not match or PO number does not exist in the IAR Form.",
                    "Invalid PO Number",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
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
            catch (MySqlException mysqlEx)
            {
                // Check for foreign key constraint violation
                if (mysqlEx.Number == 1452 || mysqlEx.Message.Contains("foreign key constraint") || 
                    mysqlEx.Message.Contains("Cannot add or update a child row"))
                {
                    MessageBox.Show(
                        "PO number does not match or PO number does not exist in the IAR Form.",
                        "Invalid PO Number",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show(
                        $"Unable to save DEV entry: {mysqlEx.Message}",
                        "Save Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
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
            byte[] documentBytes = GetDocumentBytes();
            decimal grossAmountValue = ParseCurrencyValue(grossAmount.Text);
            decimal deductionsValue = ParseCurrencyValue(deductions.Text);
            decimal netAmountValue = ParseCurrencyValue(netAmount.Text);
            string oraSerialNo = orsbursNo.Text.Trim();
            string jevNo = jev_no.Text.Trim();

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            {
                // Connection is already open from GetConnection()
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Retrieve outstanding payable from ora_burono
                        decimal outstandingAmount = GetOutstandingAmount(connection, transaction, oraSerialNo);
                        
                        // Step 2: Calculate remaining balance
                        decimal remainingBalance = outstandingAmount - grossAmountValue;
                        
                        // Validate no negative balance
                        if (remainingBalance < 0)
                        {
                            throw new InvalidOperationException("Payment amount exceeds outstanding balance. Negative values are not allowed.");
                        }
                        
                        // Step 3: Determine status
                        string paymentStatus = remainingBalance == 0 ? "Fully Paid" : "Partially Paid";
                        
                        // Step 4: Get ORA/PO information for sbl_po table (before payment)
                        // This retrieves the current balance which will be used as po_amount in sbl_po
                        var oraInfo = GetORAInfo(connection, transaction, oraSerialNo);
                        
                        // Step 5: Insert DEV entry
                        string insertDev = @"INSERT INTO dev
                            (dev_no, date, fund_cluster, ora_serialno, payee, jev_no,
                             address, dateof_jev, particulars, mode_of_payment, responsibility_center,
                             tin, mfo_pap, tax_type, gross_amount, deductions,
                             net_amount, status, approving_officer, documents)
                            VALUES
                            (@dev_no, @date, @fund_cluster, @ora_serialno, @payee, @jev_no,
                             @address, @dateof_jev, @particulars, @mode_of_payment, @responsibility_center,
                             @tin, @mfo_pap, @tax_type, @gross_amount, @deductions,
                             @net_amount, @status, @approving_officer, @documents);";

                        using (MySqlCommand cmd = new MySqlCommand(insertDev, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@dev_no", dev_no.Text.Trim());
                            cmd.Parameters.AddWithValue("@date", dev_date.Value.Date);
                            cmd.Parameters.AddWithValue("@fund_cluster", fundcluster.Text.Trim());
                            cmd.Parameters.AddWithValue("@ora_serialno", oraSerialNo);
                            cmd.Parameters.AddWithValue("@payee", payee.Text.Trim());
                            cmd.Parameters.AddWithValue("@jev_no", jevNo);
                            cmd.Parameters.AddWithValue("@address", address.Text.Trim());
                            cmd.Parameters.AddWithValue("@dateof_jev", dateofJEV.Value.Date);
                            cmd.Parameters.AddWithValue("@particulars", particulars.Text.Trim());
                            cmd.Parameters.AddWithValue("@mode_of_payment", mop.Text.Trim());
                            cmd.Parameters.AddWithValue("@responsibility_center", respcenter.Text.Trim());
                            cmd.Parameters.AddWithValue("@tin", tinNo.Text.Trim());
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

                        // Step 6: Update ora_burono status and amount (if there's a balance)
                        UpdateORAStatusAndAmount(connection, transaction, oraSerialNo, paymentStatus, remainingBalance);

                        // Step 7: Record payment in sbl_po table
                        // po_amount should be the balance before payment (outstandingAmount)
                        // balance should be the remaining balance after payment
                        InsertPaymentRecord(connection, transaction, oraInfo, jevNo, dev_no.Text.Trim(), 
                            grossAmountValue, outstandingAmount, remainingBalance, paymentStatus);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
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

        private bool ValidateNoNegativeValues(out string message)
        {
            decimal grossAmountValue = ParseCurrencyValue(grossAmount.Text);
            decimal deductionsValue = ParseCurrencyValue(deductions.Text);
            decimal netAmountValue = ParseCurrencyValue(netAmount.Text);

            if (grossAmountValue < 0)
            {
                message = "Invalid payment amount. Negative values are not allowed.";
                return false;
            }

            if (deductionsValue < 0)
            {
                message = "Invalid deductions amount. Negative values are not allowed.";
                return false;
            }

            if (netAmountValue < 0)
            {
                message = "Invalid net amount. Negative values are not allowed.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private bool ValidateJEVExists(string jevNo, out string message)
        {
            if (string.IsNullOrWhiteSpace(jevNo))
            {
                message = "JEV Number is required.";
                return false;
            }

            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT COUNT(*) FROM jev WHERE jev_no = @jev_no";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@jev_no", jevNo.Trim());
                        object result = command.ExecuteScalar();
                        int count = Convert.ToInt32(result);
                        
                        if (count == 0)
                        {
                            message = "JEV number does not exist. Please enter a valid JEV number.";
                            return false;
                        }
                    }
                }

                message = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                message = $"Error validating JEV: {ex.Message}";
                return false;
            }
        }

        private bool ValidateJEVGrossAmount(string jevNo, decimal grossAmount, out string message)
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT gross_amount FROM jev WHERE jev_no = @jev_no LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@jev_no", jevNo.Trim());
                        object result = command.ExecuteScalar();
                        
                        if (result == null || result == DBNull.Value)
                        {
                            message = "JEV record not found.";
                            return false;
                        }

                        decimal jevGrossAmount = Convert.ToDecimal(result);
                        
                        if (jevGrossAmount != grossAmount)
                        {
                            message = $"Gross amount ({grossAmount:N2}) does not match the JEV gross amount ({jevGrossAmount:N2}).";
                            return false;
                        }
                    }
                }

                message = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                message = $"Error validating JEV gross amount: {ex.Message}";
                return false;
            }
        }

        private bool ValidateORASerialNoExists(string oraSerialNo, out string message)
        {
            if (string.IsNullOrWhiteSpace(oraSerialNo))
            {
                message = "ORA Serial Number is required.";
                return false;
            }

            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT COUNT(*) FROM ora_burono WHERE ora_serialno = @ora_serialno";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ora_serialno", oraSerialNo.Trim());
                        object result = command.ExecuteScalar();
                        int count = Convert.ToInt32(result);
                        
                        if (count == 0)
                        {
                            message = "ORA Serial Number does not exist.";
                            return false;
                        }
                    }
                }

                message = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                message = $"Error validating ORA Serial Number: {ex.Message}";
                return false;
            }
        }

        private decimal GetOutstandingAmount(MySqlConnection connection, MySqlTransaction transaction, string oraSerialNo)
        {
            // Retrieve the current balance (outstanding payable) from ora_burono
            string query = @"SELECT balance FROM ora_burono WHERE ora_serialno = @ora_serialno LIMIT 1";

            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@ora_serialno", oraSerialNo);
                object result = command.ExecuteScalar();
                
                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("ORA Serial Number not found in ora_burono table.");
                }

                decimal balance = Convert.ToDecimal(result);
                
                // Ensure balance is not negative
                if (balance < 0)
                {
                    throw new InvalidOperationException("Invalid outstanding balance. Negative values are not allowed.");
                }

                return balance;
            }
        }

        private (string poNo, string payee, decimal balance) GetORAInfo(MySqlConnection connection, MySqlTransaction transaction, string oraSerialNo)
        {
            // Retrieve PO number, payee (supplier), and current balance for sbl_po table
            // po_amount in sbl_po should be the balance before payment (current balance)
            string query = @"SELECT po_no, payee, balance FROM ora_burono WHERE ora_serialno = @ora_serialno LIMIT 1";

            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@ora_serialno", oraSerialNo);
                
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string poNo = reader["po_no"]?.ToString() ?? "";
                        string payee = reader["payee"]?.ToString() ?? "";
                        decimal balance = reader["balance"] == DBNull.Value ? 0m : reader.GetDecimal("balance");
                        
                        // Ensure balance is not negative
                        if (balance < 0)
                        {
                            throw new InvalidOperationException("Invalid balance in ora_burono. Negative values are not allowed.");
                        }
                        
                        return (poNo, payee, balance);
                    }
                }
            }

            throw new InvalidOperationException("ORA Serial Number not found in ora_burono table.");
        }

        private void UpdateORAStatusAndAmount(MySqlConnection connection, MySqlTransaction transaction, string oraSerialNo, string status, decimal remainingBalance)
        {
            // Update ora_burono status and balance based on payment
            // If fully paid (remainingBalance = 0), update status and set balance to 0
            // If partially paid (remainingBalance > 0), update both status and balance
            
            // Ensure remaining balance is not negative
            if (remainingBalance < 0)
            {
                throw new InvalidOperationException("Invalid remaining balance. Negative values are not allowed.");
            }

            string query = @"UPDATE ora_burono SET status = @status, balance = @balance WHERE ora_serialno = @ora_serialno";

            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@status", status);
                command.Parameters.AddWithValue("@balance", remainingBalance);
                command.Parameters.AddWithValue("@ora_serialno", oraSerialNo);
                command.ExecuteNonQuery();
            }
        }

        private void InsertPaymentRecord(MySqlConnection connection, MySqlTransaction transaction, 
            (string poNo, string payee, decimal balance) oraInfo, string jevNo, string devNo, 
            decimal amountPaid, decimal poAmount, decimal remainingBalance, string status)
        {
            // Insert payment history record into sbl_po table
            // po_amount: balance before payment (outstandingAmount)
            // amount_paid: gross amount from DEV entry (payment amount)
            // balance: remaining balance after payment
            // status: "Fully Paid" or "Partially Paid"
            
            // Ensure no negative values
            if (poAmount < 0 || amountPaid < 0 || remainingBalance < 0)
            {
                throw new InvalidOperationException("Invalid payment amount. Negative values are not allowed.");
            }

            string query = @"INSERT INTO sbl_po
                (po_no, supplier, ora_serialno, po_amount, responsibility_code, dev_no, jev_no,
                 checkNo, date_paid, amount_paid, balance, status)
                VALUES
                (@po_no, @supplier, @ora_serialno, @po_amount, @responsibility_code, @dev_no, @jev_no,
                 @checkNo, @date_paid, @amount_paid, @balance, @status)";

            using (MySqlCommand command = new MySqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@po_no", oraInfo.poNo);
                command.Parameters.AddWithValue("@supplier", oraInfo.payee);
                command.Parameters.AddWithValue("@ora_serialno", orsbursNo.Text.Trim());
                command.Parameters.AddWithValue("@po_amount", poAmount); // Balance before payment
                command.Parameters.AddWithValue("@responsibility_code", respcenter.Text.Trim());
                command.Parameters.AddWithValue("@dev_no", devNo);
                command.Parameters.AddWithValue("@jev_no", jevNo);
                // Note: checkNo field - using mfopap as placeholder, but should be from a dedicated checkNo field if available
                // Note: checkNo is currently using mfopap field as per requirements
                // If a dedicated checkNo field exists in the form, it should be used instead
                command.Parameters.AddWithValue("@checkNo", mfopap.Text.Trim());
                command.Parameters.AddWithValue("@date_paid", dev_date.Value.Date);
                command.Parameters.AddWithValue("@amount_paid", amountPaid); // Gross amount from DEV (payment amount)
                command.Parameters.AddWithValue("@balance", remainingBalance); // Remaining balance after payment
                command.Parameters.AddWithValue("@status", status);
                command.ExecuteNonQuery();
            }
        }
    }
}
