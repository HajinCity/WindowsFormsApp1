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
    public partial class AddIARForm : Form
    {
        // File upload fields
        private readonly List<FileInfo> uploadedFiles = new List<FileInfo>();
        private Panel fileListPanel;
        private Color originalPanelColor;
        private Point originalPanel1Location;
        private Size originalPanel1Size;
        private int originalCreateButtonTop;
        private int originalCancelButtonTop;

        private const long MAX_FILE_SIZE = 20 * 1024 * 1024; // 20 MB
        private readonly string[] allowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };

        public AddIARForm()
        {
            InitializeComponent();
            InitializeFileUpload();
            createIAREntryBtn.Click += CreateIAREntryBtn_Click;
            cancel.Click += (s, e) => this.Close();
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
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
            originalPanel1Location = panel1.Location;
            originalPanel1Size = panel1.Size;
            originalCreateButtonTop = createIAREntryBtn.Top;
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
                Location = originalPanel1Location,
                Size = originalPanel1Size,
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
                panel1.Location = originalPanel1Location;
                panel1.Size = originalPanel1Size;
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
            createIAREntryBtn.Top = originalCreateButtonTop;
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
            int panelWidth = originalPanel1Size.Width;
            
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

        public List<FileInfo> GetUploadedFiles()
        {
            return new List<FileInfo>(uploadedFiles);
        }

        private void CreateIAREntryBtn_Click(object sender, EventArgs e)
        {
            if (!AreMainFieldsValid(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!AreStockRowsValid(out string stockMessage))
            {
                MessageBox.Show(stockMessage, "Invalid Stock Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to save this IAR entry?",
                "Confirm Save",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SaveIarEntry();
                MessageBox.Show("Inspection and Acceptance Report saved successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to save IAR entry: {ex.Message}",
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool AreMainFieldsValid(out string message)
        {
            var requiredFields = new List<(string Value, string Label)>
            {
                (iarNo.Text, "IAR Number"),
                (supplierName.Text, "Supplier Name"),
                (RequisitioningOffice.Text, "Requisitioning Office"),
                (FundCluster.Text, "Fund Cluster"),
                (RequistioningCnterCode.Text, "Requisitioning Center Code"),
                (po_number.Text, "PO Number"),
                (InvoiceNumber.Text, "Invoice Number"),
                (InspectorOfficer.Text, "Inspector Officer"),
                (PropertyCustodianOfficer.Text, "Property Custodian Officer"),
                (ReceivedStatus.Text, "Received Status"),
                (TotalAmount.Text, "Total Amount"),
                (textBox11.Text, "Remarks")
            };

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    message = $"{field.Label} is required.";
                    return false;
                }
            }

            if (!decimal.TryParse(TotalAmount.Text, out _))
            {
                message = "Total Amount must be a valid number.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private bool AreStockRowsValid(out string message)
        {
            if (dataGridView1.Rows.Count == 0 ||
                (dataGridView1.Rows.Count == 1 && dataGridView1.Rows[0].IsNewRow))
            {
                message = "Please add at least one stock item.";
                return false;
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                string stockNumber = row.Cells[0].Value?.ToString();
                string description = row.Cells[1].Value?.ToString();
                string unit = row.Cells[2].Value?.ToString();
                string quantityValue = row.Cells[3].Value?.ToString();

                if (string.IsNullOrWhiteSpace(stockNumber) ||
                    string.IsNullOrWhiteSpace(description) ||
                    string.IsNullOrWhiteSpace(unit) ||
                    string.IsNullOrWhiteSpace(quantityValue))
                {
                    message = "All stock item columns must be filled.";
                    return false;
                }

                if (!decimal.TryParse(quantityValue, out _))
                {
                    message = $"Quantity '{quantityValue}' must be a valid number.";
                    return false;
                }
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

        private void SaveIarEntry()
        {
            byte[] documentBytes = GetDocumentBytes();
            decimal totalAmountValue = decimal.Parse(TotalAmount.Text);

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            using (MySqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    long iarId;
                    string insertIar = @"INSERT INTO inspection_acceptance_report
                        (iar_no, date_iar, fund_cluster, supplier, po_no, po_date,
                         requisitioning_office, responsibility_centercode, `Invoice No`,
                         date_inspected, inspector_offcer, date_received,
                         property_custodian_officer, record_status, total_amount,
                         remarks, documents)
                        VALUES
                        (@iar_no, @date_iar, @fund_cluster, @supplier, @po_no, @po_date,
                         @requisitioning_office, @responsibility_centercode, @invoice_no,
                         @date_inspected, @inspector_offcer, @date_received,
                         @property_custodian_officer, @record_status, @total_amount,
                         @remarks, @documents);";

                    using (MySqlCommand cmd = new MySqlCommand(insertIar, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@iar_no", iarNo.Text.Trim());
                        cmd.Parameters.AddWithValue("@date_iar", iar_date.Value.Date);
                        cmd.Parameters.AddWithValue("@fund_cluster", FundCluster.Text.Trim());
                        cmd.Parameters.AddWithValue("@supplier", supplierName.Text.Trim());
                        cmd.Parameters.AddWithValue("@po_no", po_number.Text.Trim());
                        cmd.Parameters.AddWithValue("@po_date", po_date.Value.Date);
                        cmd.Parameters.AddWithValue("@requisitioning_office", RequisitioningOffice.Text.Trim());
                        cmd.Parameters.AddWithValue("@responsibility_centercode", RequistioningCnterCode.Text.Trim());
                        cmd.Parameters.AddWithValue("@invoice_no", InvoiceNumber.Text.Trim());
                        cmd.Parameters.AddWithValue("@date_inspected", date_inspected.Value.Date);
                        cmd.Parameters.AddWithValue("@inspector_offcer", InspectorOfficer.Text.Trim());
                        cmd.Parameters.AddWithValue("@date_received", date_received.Value.Date);
                        cmd.Parameters.AddWithValue("@property_custodian_officer", PropertyCustodianOfficer.Text.Trim());
                        cmd.Parameters.AddWithValue("@record_status", ReceivedStatus.Text.Trim());
                        cmd.Parameters.AddWithValue("@total_amount", totalAmountValue);
                        cmd.Parameters.AddWithValue("@remarks", textBox11.Text.Trim());

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
                        iarId = cmd.LastInsertedId;
                    }

                    string insertStock = @"INSERT INTO stock_property
                        (iar_id, stck_no, description, unit, quantity)
                        VALUES
                        (@iar_id, @stck_no, @description, @unit, @quantity);";

                    using (MySqlCommand stockCmd = new MySqlCommand(insertStock, connection, transaction))
                    {
                        stockCmd.Parameters.Add("@iar_id", MySqlDbType.Int64);
                        stockCmd.Parameters.Add("@stck_no", MySqlDbType.VarChar);
                        stockCmd.Parameters.Add("@description", MySqlDbType.VarChar);
                        stockCmd.Parameters.Add("@unit", MySqlDbType.VarChar);
                        stockCmd.Parameters.Add("@quantity", MySqlDbType.Decimal);

                        foreach (DataGridViewRow row in dataGridView1.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            stockCmd.Parameters["@iar_id"].Value = iarId;
                            stockCmd.Parameters["@stck_no"].Value = row.Cells[0].Value?.ToString().Trim();
                            stockCmd.Parameters["@description"].Value = row.Cells[1].Value?.ToString().Trim();
                            stockCmd.Parameters["@unit"].Value = row.Cells[2].Value?.ToString().Trim();
                            stockCmd.Parameters["@quantity"].Value = decimal.Parse(row.Cells[3].Value.ToString());
                            stockCmd.ExecuteNonQuery();
                        }
                    }

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
}