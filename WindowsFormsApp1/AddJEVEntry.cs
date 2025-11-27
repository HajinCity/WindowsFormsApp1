using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    public partial class AddJEVEntry : Form
    {
        // File upload fields
        private readonly List<FileInfo> uploadedFiles = new List<FileInfo>();
        private Panel fileListPanel;
        private Color originalPanelColor;
        private Point originalPanelLocation;
        private Size originalPanelSize;
        private int originalCreateButtonTop;
        private int originalCancelButtonTop;

        private const long MAX_FILE_SIZE = 20 * 1024 * 1024; // 20 MB
        private readonly string[] allowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };

        public AddJEVEntry()
        {
            InitializeComponent();
            InitializeFileUpload();
            InitializeCalculation();
            CreateJournalEntryBtn.Click += CreateJournalEntryBtn_Click;
            cancel.Click += (s, e) => this.Close();
        }

        private void InitializeCalculation()
        {
            // Make netAmount read-only
            netAmount.ReadOnly = true;
            netAmount.BackColor = Color.WhiteSmoke; // Visual indication that it's read-only

            // Add numeric-only input handlers
            grossAmount.KeyPress += NumericTextBox_KeyPress;
            deductions.KeyPress += NumericTextBox_KeyPress;

            // Add calculation handlers
            grossAmount.TextChanged += CalculateNetAmount;
            deductions.TextChanged += CalculateNetAmount;
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow digits, decimal point, and control characters (backspace, delete, etc.)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            // Only allow one decimal point
            TextBox textBox = sender as TextBox;
            if (e.KeyChar == '.' && textBox != null && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void CalculateNetAmount(object sender, EventArgs e)
        {
            try
            {
                decimal gross = 0;
                decimal deduct = 0;
                bool canCalculate = true;

                // Parse grossAmount
                if (!string.IsNullOrWhiteSpace(grossAmount.Text))
                {
                    if (decimal.TryParse(grossAmount.Text, out decimal grossValue))
                    {
                        gross = grossValue;
                    }
                    else
                    {
                        canCalculate = false;
                    }
                }

                // Parse deductions
                if (!string.IsNullOrWhiteSpace(deductions.Text))
                {
                    if (decimal.TryParse(deductions.Text, out decimal deductValue))
                    {
                        deduct = deductValue;
                    }
                    else
                    {
                        canCalculate = false;
                    }
                }

                // Calculate and display net amount if both fields are valid or empty
                if (canCalculate)
                {
                    decimal net = gross - deduct;
                    netAmount.Text = net.ToString("0.00");
                }
                else
                {
                    // Invalid input, clear net amount
                    netAmount.Text = "";
                }
            }
            catch
            {
                // If calculation fails, clear the net amount
                netAmount.Text = "";
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InitializeFileUpload()
        {
            if (panel1 == null)
            {
                return;
            }

            originalPanelColor = panel1.BackColor;
            originalPanelLocation = panel1.Location;
            originalPanelSize = panel1.Size;
            originalCreateButtonTop = CreateJournalEntryBtn.Top;
            originalCancelButtonTop = cancel.Top;

            panel1.AllowDrop = true;
            panel1.DragEnter += Panel1_DragEnter;
            panel1.DragOver += Panel1_DragOver;
            panel1.DragLeave += Panel1_DragLeave;
            panel1.DragDrop += Panel1_DragDrop;
            panel1.Click += Panel1_Click;
            panel1.Cursor = Cursors.Hand;

            foreach (Control control in panel1.Controls)
            {
                control.AllowDrop = true;
                control.DragEnter += Panel1_DragEnter;
                control.DragOver += Panel1_DragOver;
                control.DragLeave += Panel1_DragLeave;
                control.DragDrop += Panel1_DragDrop;
                control.Click += Panel1_Click;
                control.Cursor = Cursors.Hand;
            }

            fileListPanel = new Panel
            {
                Location = originalPanelLocation,
                Size = originalPanelSize,
                AutoSize = false,
                AutoScroll = true,
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right // No bottom anchor to prevent sticking to bottom
            };

            // Insert at the same index as panel1 to ensure proper Z-order
            int panel1Index = Controls.IndexOf(panel1);
            if (panel1Index >= 0)
            {
                Controls.Add(fileListPanel);
                Controls.SetChildIndex(fileListPanel, panel1Index + 1);
            }
            else
            {
                Controls.Add(fileListPanel);
            }
            fileListPanel.BringToFront();
        }

        private void Panel1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Supported Files|*.pdf;*.png;*.jpg;*.jpeg;*.docx|All Files|*.*";
                dialog.Multiselect = false;
                dialog.Title = "Select Supporting Document";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessFiles(dialog.FileNames);
                }
            }
        }

        private void Panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                panel1.BackColor = Color.FromArgb(240, 248, 255);
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Panel1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void Panel1_DragLeave(object sender, EventArgs e)
        {
            panel1.BackColor = originalPanelColor;
        }

        private void Panel1_DragDrop(object sender, DragEventArgs e)
        {
            panel1.BackColor = originalPanelColor;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                if (files.Length > 1)
                {
                    MessageBox.Show(
                        "Only one file can be uploaded at a time. The first file will be used.",
                        "Multiple Files",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                ProcessFiles(new[] { files[0] });
            }
        }

        private void ProcessFiles(string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
            {
                return;
            }

            string filePath = filePaths[0];

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                string extension = fileInfo.Extension.ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    MessageBox.Show(
                        $"File '{fileInfo.Name}' has an unsupported file type. Supported types: PDF, PNG, JPG, DOCX",
                        "Invalid File Type",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (fileInfo.Length > MAX_FILE_SIZE)
                {
                    MessageBox.Show(
                        $"File '{fileInfo.Name}' exceeds the maximum file size of 20 MB.",
                        "File Too Large",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                uploadedFiles.Clear();
                uploadedFiles.Add(fileInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error processing file '{filePath}': {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }

            UpdateFileList();
        }

        private void UpdateFileList()
        {
            SuspendLayout();
            
            try
            {
                Point scrollPosition = AutoScrollPosition;

                fileListPanel.Controls.Clear();

                if (uploadedFiles.Count == 0)
                {
                    fileListPanel.Controls.Clear();
                    fileListPanel.Height = 0; // shrink
                    fileListPanel.Visible = false;
                    panel1.Visible = true;
                    panel1.Location = originalPanelLocation;
                    panel1.Size = originalPanelSize;
                    AutoScrollPosition = new Point(Math.Abs(scrollPosition.X), Math.Abs(scrollPosition.Y));
                    return;
                }

                // Get current location and size of panel1 before hiding it (in case form was resized/scrolled)
                Point currentPanelLocation = panel1.Location;
                Size currentPanelSize = panel1.Size;

                // Hide panel1 completely to remove the gap
                panel1.Visible = false;

                // Position file list exactly where panel1 was - use absolute coordinates
                fileListPanel.Location = currentPanelLocation;
                fileListPanel.Size = new Size(currentPanelSize.Width, 0); // Start with 0 height
                fileListPanel.Visible = true;
                // Set anchor to prevent panel from moving (no bottom anchor)
                fileListPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                int yPosition = 0;
                foreach (FileInfo file in uploadedFiles)
                {
                    Panel fileItemPanel = CreateFileItemPanel(file, yPosition);
                    fileListPanel.Controls.Add(fileItemPanel);
                    yPosition += fileItemPanel.Height + 5;
                }
                fileListPanel.Height = yPosition; // Auto resize panel to exact height

                // Ensure buttons stay in their original positions
                CreateJournalEntryBtn.Top = originalCreateButtonTop;
                cancel.Top = originalCancelButtonTop;

                // Ensure it's in the correct Z-order and visible
                fileListPanel.BringToFront();
                AutoScrollPosition = new Point(Math.Abs(scrollPosition.X), Math.Abs(scrollPosition.Y));
            }
            finally
            {
                ResumeLayout(true);
                fileListPanel.Refresh(); // Force a refresh to ensure it's displayed
            }
        }

        private Panel CreateFileItemPanel(FileInfo file, int yPosition)
        {
            const int panelHeight = 50;
            const int horizontalPadding = 15;
            int panelWidth = originalPanelSize.Width;
            
            Panel itemPanel = new Panel
            {
                Location = new Point(0, yPosition),
                Size = new Size(panelWidth, panelHeight),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right // Fill width
            };

            // File name label - left aligned
            Label fileNameLabel = new Label
            {
                Text = file.Name,
                Location = new Point(horizontalPadding, (panelHeight - 20) / 2), // Vertically centered
                Size = new Size(400, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoEllipsis = true // Truncate with ellipsis if too long
            };
            itemPanel.Controls.Add(fileNameLabel);

            // File type label - positioned after file name
            Label fileTypeLabel = new Label
            {
                Text = file.Extension.ToUpper().TrimStart('.'),
                Location = new Point(420, (panelHeight - 20) / 2), // Vertically centered
                Size = new Size(60, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(128, 128, 128), // Lighter gray
                TextAlign = ContentAlignment.MiddleLeft
            };
            itemPanel.Controls.Add(fileTypeLabel);

            // File size label - positioned after file type
            Label fileSizeLabel = new Label
            {
                Text = FormatFileSize(file.Length),
                Location = new Point(490, (panelHeight - 20) / 2), // Vertically centered
                Size = new Size(100, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(128, 128, 128), // Lighter gray
                TextAlign = ContentAlignment.MiddleLeft
            };
            itemPanel.Controls.Add(fileSizeLabel);

            // Remove button - right aligned
            Button removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(panelWidth - 100 - horizontalPadding, (panelHeight - 30) / 2), // Vertically centered, right aligned
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Arial", 9, FontStyle.Regular),
                Anchor = AnchorStyles.Top | AnchorStyles.Right // Keep right aligned when panel resizes
            };
            removeButton.FlatAppearance.BorderSize = 0;
            removeButton.Click += (s, e) => RemoveFile(file);
            itemPanel.Controls.Add(removeButton);

            return itemPanel;
        }

        private void RemoveFile(FileInfo file)
        {
            uploadedFiles.Remove(file);
            if (uploadedFiles.Count == 0)
            {
                fileListPanel.Controls.Clear();
                fileListPanel.Height = 0; // shrink
                fileListPanel.Visible = false;
            }
            UpdateFileList();
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        private void CreateJournalEntryBtn_Click(object sender, EventArgs e)
        {
            if (!AreInputsValid(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Empty Fields", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to save this JEV entry?",
                "Confirm Save",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SaveJEVEntry();
                MessageBox.Show("Journal Entry Voucher saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to save JEV entry: {ex.Message}",
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool AreInputsValid(out string message)
        {
            var requiredFields = new List<(string Value, string Label)>
            {
                (jev_no.Text, "JEV No."),
                (rspCode.Text, "Responsibility Center Code"),
                (uacscode.Text, "UACS Code"),
                (account.Text, "Account"),
                (particulars.Text, "Particulars"),
                (taxtype.Text, "Tax Type"),
                (grossAmount.Text, "Gross Amount"),
                (deductions.Text, "Deductions"),
                (netAmount.Text, "Net Amount"),
                (status.Text, "Status"),
                (approvingOfficer.Text, "Approving Officer")
            };

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    message = $"{field.Label} is required and cannot be empty.";
                    return false;
                }
            }

            // Validate decimal fields
            if (!decimal.TryParse(grossAmount.Text, out _))
            {
                message = "Gross Amount must be a valid number.";
                return false;
            }

            if (!decimal.TryParse(deductions.Text, out _))
            {
                message = "Deductions must be a valid number.";
                return false;
            }

            if (!decimal.TryParse(netAmount.Text, out _))
            {
                message = "Net Amount must be a valid number.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private byte[] GetDocumentBytes()
        {
            if (uploadedFiles.Count == 0)
            {
                return null;
            }

            try
            {
                return File.ReadAllBytes(uploadedFiles[0].FullName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to read the uploaded document: {ex.Message}", ex);
            }
        }

        private void SaveJEVEntry()
        {
            byte[] documentBytes = GetDocumentBytes();
            decimal grossAmountValue = decimal.Parse(grossAmount.Text);
            decimal deductionsValue = decimal.Parse(deductions.Text);
            decimal netAmountValue = decimal.Parse(netAmount.Text);

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            {
                string insertJev = @"INSERT INTO jev
                    (jev_no, date, responsibility_center, uacs_code, account, particulars,
                     gross_amount, deductions, tax_type, net_amount, status, approving_officer, documents)
                    VALUES
                    (@jev_no, @date, @responsibility_center, @uacs_code, @account, @particulars,
                     @gross_amount, @deductions, @tax_type, @net_amount, @status, @approving_officer, @documents);";

                using (MySqlCommand cmd = new MySqlCommand(insertJev, connection))
                {
                    cmd.Parameters.AddWithValue("@jev_no", jev_no.Text.Trim());
                    cmd.Parameters.AddWithValue("@date", jevDate.Value.Date);
                    cmd.Parameters.AddWithValue("@responsibility_center", rspCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@uacs_code", uacscode.Text.Trim());
                    cmd.Parameters.AddWithValue("@account", account.Text.Trim());
                    cmd.Parameters.AddWithValue("@particulars", particulars.Text.Trim());
                    cmd.Parameters.AddWithValue("@gross_amount", grossAmountValue);
                    cmd.Parameters.AddWithValue("@deductions", deductionsValue);
                    cmd.Parameters.AddWithValue("@tax_type", taxtype.Text.Trim());
                    cmd.Parameters.AddWithValue("@net_amount", netAmountValue);
                    cmd.Parameters.AddWithValue("@status", status.Text.Trim());
                    cmd.Parameters.AddWithValue("@approving_officer", approvingOfficer.Text.Trim());

                    var documentParam = cmd.Parameters.Add("@documents", MySqlDbType.LongBlob);
                    if (documentBytes == null || documentBytes.Length == 0)
                    {
                        documentParam.Value = DBNull.Value;
                    }
                    else
                    {
                        documentParam.Value = documentBytes;
                    }

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
