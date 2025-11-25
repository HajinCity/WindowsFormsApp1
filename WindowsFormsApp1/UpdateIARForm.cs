using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class UpdateIARForm : Form
    {
        private readonly int iarId;
        private readonly bool isEditMode;
        private byte[] documentBytes;
        private byte[] originalDocumentBytes;
        private string storedDocumentExtension;
        private string uploadedDocumentName;
        private string uploadedDocumentExtension;

        private readonly List<FileInfo> uploadedFiles = new List<FileInfo>();
        private Panel fileListPanel;
        private Color originalPanelColor;
        private Point originalPanel1Location;
        private Size originalPanel1Size;

        private Button downloadDocumentButton;
        private Button changeDocumentButton;
        private Button removeDocumentButton;
        private Label documentStatusLabel;

        private const long MAX_FILE_SIZE = 20 * 1024 * 1024;
        private readonly string[] allowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };

        public UpdateIARForm()
        {
            InitializeComponent();
            InitializeFileUpload();
            InitializeDocumentControls();
            UpdateIARbtn.Click += UpdateIARbtn_Click;
            cancel.Click += (s, e) => this.Close();
        }

        public UpdateIARForm(int iarId) : this()
        {
            this.iarId = iarId;
            isEditMode = iarId > 0;
            if (isEditMode)
            {
                LoadIarDetails();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InitializeFileUpload()
        {
            originalPanelColor = panel1.BackColor;
            originalPanel1Location = panel1.Location;
            originalPanel1Size = panel1.Size;

            panel1.AllowDrop = true;
            panel1.DragEnter += Panel1_DragEnter;
            panel1.DragOver += Panel1_DragOver;
            panel1.DragLeave += Panel1_DragLeave;
            panel1.DragDrop += Panel1_DragDrop;
            panel1.Click += Panel1_Click;
            panel1.Cursor = Cursors.Hand;

            foreach (Control control in new Control[] { pictureBox1, label18, label19 })
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
                AutoScroll = true,
                Visible = false
            };
            fileListPanel.Anchor = panel1.Anchor;
            Controls.Add(fileListPanel);
            fileListPanel.BringToFront();
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

            changeDocumentButton = new Button
            {
                Text = "Replace Document",
                AutoSize = false,
                Size = new Size(220, 35),
                BackColor = Color.White,
                ForeColor = Color.SeaGreen,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            changeDocumentButton.FlatAppearance.BorderSize = 1;
            changeDocumentButton.FlatAppearance.BorderColor = Color.SeaGreen;
            changeDocumentButton.Cursor = Cursors.Hand;
            changeDocumentButton.Click += ChangeDocumentButton_Click;
            panel1.Controls.Add(changeDocumentButton);

            removeDocumentButton = new Button
            {
                Text = "Remove Document",
                AutoSize = false,
                Size = new Size(220, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            removeDocumentButton.FlatAppearance.BorderSize = 0;
            removeDocumentButton.Cursor = Cursors.Hand;
            removeDocumentButton.Click += RemoveDocumentButton_Click;
            panel1.Controls.Add(removeDocumentButton);

            documentStatusLabel = new Label
            {
                Text = "A document is attached to this entry.",
                AutoSize = true,
                ForeColor = Color.FromArgb(64, 64, 64),
                Visible = false
            };
            panel1.Controls.Add(documentStatusLabel);

            panel1.Resize += Panel1_Resize;
            PositionDocumentControls();
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

                documentBytes = File.ReadAllBytes(filePath);
                uploadedDocumentName = fileInfo.Name;
                uploadedDocumentExtension = fileInfo.Extension;
                storedDocumentExtension = uploadedDocumentExtension;

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

            UpdateDocumentSection();
        }

        private void UpdateDocumentSection()
        {
            bool hasNewUpload = uploadedFiles.Count > 0;
            bool hasDocument = documentBytes != null && documentBytes.Length > 0;

            if (hasNewUpload)
            {
                ShowFileList();
                downloadDocumentButton.Visible = true;
                changeDocumentButton.Visible = true;
                removeDocumentButton.Visible = true;
                documentStatusLabel.Visible = true;
                documentStatusLabel.Text = $"Ready to upload: {uploadedDocumentName}";
                downloadDocumentButton.Text = "Download Selected Document";
                changeDocumentButton.Text = "Choose Different Document";
                return;
            }

            fileListPanel.Visible = false;
            panel1.Visible = true;

            if (hasDocument)
            {
                pictureBox1.Visible = false;
                label18.Text = string.Empty;
                label19.Text = string.Empty;

                downloadDocumentButton.Visible = true;
                changeDocumentButton.Visible = true;
                removeDocumentButton.Visible = true;
                documentStatusLabel.Visible = true;
                documentStatusLabel.Text = "Document available. Use the buttons below.";
                downloadDocumentButton.Text = "Download / View Document";
                changeDocumentButton.Text = "Replace Document";
            }
            else
            {
                pictureBox1.Visible = true;
                label18.Text = "Click to upload or drag and drop";
                label19.Text = "PDF,JPG,PNG,DOCX (20MB)";

                downloadDocumentButton.Visible = false;
                changeDocumentButton.Visible = false;
                removeDocumentButton.Visible = false;
                documentStatusLabel.Visible = false;
            }

            PositionDocumentControls();
        }

        private void ShowFileList()
        {
            panel1.Visible = false;
            fileListPanel.Visible = true;
            fileListPanel.Location = originalPanel1Location;
            fileListPanel.Controls.Clear();
            fileListPanel.Height = 0;  // Reset before adding files
            fileListPanel.Width = originalPanel1Size.Width;

            int yPosition = 0;
            foreach (FileInfo file in uploadedFiles)
            {
                Panel fileItemPanel = CreateFileItemPanel(file, yPosition);
                fileListPanel.Controls.Add(fileItemPanel);
                yPosition += fileItemPanel.Height + 5;
            }
            fileListPanel.Height = yPosition; // Auto resize panel to exact height
        }

        private Panel CreateFileItemPanel(FileInfo file, int yPosition)
        {
            Panel itemPanel = new Panel
            {
                Location = new Point(0, yPosition),
                Size = new Size(originalPanel1Size.Width, 50),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label fileNameLabel = new Label
            {
                Text = file.Name,
                Location = new Point(10, 15),
                Size = new Size(300, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Black
            };
            itemPanel.Controls.Add(fileNameLabel);

            Label fileType = new Label
            {
                Text = file.Extension.ToUpper().TrimStart('.'),
                Location = new Point(320, 15),
                Size = new Size(80, 20),
                ForeColor = Color.Gray
            };
            itemPanel.Controls.Add(fileType);

            Label fileSize = new Label
            {
                Text = FormatFileSize(file.Length),
                Location = new Point(410, 15),
                Size = new Size(100, 20),
                ForeColor = Color.Gray
            };
            itemPanel.Controls.Add(fileSize);

            Button removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(600, 10),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
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
                panel1.Visible = true;   // original layout restored
                uploadedDocumentName = null;
                uploadedDocumentExtension = null;
                documentBytes = originalDocumentBytes;
                storedDocumentExtension = GuessFileExtension(originalDocumentBytes);
            }
            else
            {
                ShowFileList();
            }
            UpdateDocumentSection();
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

            if (changeDocumentButton != null)
            {
                changeDocumentButton.Location = new Point(centerX, 65);
            }

            if (removeDocumentButton != null)
            {
                removeDocumentButton.Location = new Point(centerX, 105);
            }

            if (documentStatusLabel != null)
            {
                documentStatusLabel.Location = new Point(
                    Math.Max(0, (panel1.Width - documentStatusLabel.Width) / 2),
                    150);
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

        private void ChangeDocumentButton_Click(object sender, EventArgs e)
        {
            Panel1_Click(sender, e);
        }

        private void RemoveDocumentButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Remove the currently attached document?",
                "Remove Document",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                uploadedFiles.Clear();
                uploadedDocumentName = null;
                uploadedDocumentExtension = null;
                documentBytes = null;
                originalDocumentBytes = null;
                storedDocumentExtension = null;
                UpdateDocumentSection();
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
                            originalDocumentBytes = documentBytes;
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

        private void UpdateIARbtn_Click(object sender, EventArgs e)
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
                "Are you sure you want to update this IAR entry?",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SaveUpdates();
                MessageBox.Show("Inspection and Acceptance Report updated successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update IAR entry: {ex.Message}",
                    "Update Error",
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

        private void SaveUpdates()
        {
            decimal totalAmountValue = decimal.Parse(TotalAmount.Text);
            byte[] attachment = documentBytes;

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            using (MySqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    string updateIar = @"UPDATE inspection_acceptance_report SET
                                            iar_no = @iar_no,
                                            date_iar = @date_iar,
                                            fund_cluster = @fund_cluster,
                                            supplier = @supplier,
                                            po_no = @po_no,
                                            po_date = @po_date,
                                            requisitioning_office = @requisitioning_office,
                                            responsibility_centercode = @responsibility_centercode,
                                            `Invoice No` = @invoice_no,
                                            date_inspected = @date_inspected,
                                            inspector_offcer = @inspector_offcer,
                                            date_received = @date_received,
                                            property_custodian_officer = @property_custodian_officer,
                                            record_status = @record_status,
                                            total_amount = @total_amount,
                                            remarks = @remarks,
                                            documents = @documents
                                         WHERE iar_id = @iar_id";

                    using (MySqlCommand cmd = new MySqlCommand(updateIar, connection, transaction))
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
                        cmd.Parameters.AddWithValue("@iar_id", iarId);

                        var documentParam = cmd.Parameters.Add("@documents", MySqlDbType.LongBlob);
                        documentParam.Value = (attachment == null || attachment.Length == 0) ? (object)DBNull.Value : attachment;

                        cmd.ExecuteNonQuery();
                    }

                    using (MySqlCommand deleteCmd = new MySqlCommand("DELETE FROM stock_property WHERE iar_id = @iar_id", connection, transaction))
                    {
                        deleteCmd.Parameters.AddWithValue("@iar_id", iarId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    string insertStock = @"INSERT INTO stock_property
                        (iar_id, stck_no, description, unit, quantity)
                        VALUES
                        (@iar_id, @stck_no, @description, @unit, @quantity);";

                    using (MySqlCommand stockCmd = new MySqlCommand(insertStock, connection, transaction))
                    {
                        stockCmd.Parameters.Add("@iar_id", MySqlDbType.Int32);
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
            string extension = uploadedDocumentExtension;
            if (string.IsNullOrEmpty(extension))
            {
                extension = storedDocumentExtension;
            }
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

            string baseName = !string.IsNullOrWhiteSpace(uploadedDocumentName)
                ? Path.GetFileNameWithoutExtension(uploadedDocumentName)
                : (string.IsNullOrWhiteSpace(iarNo.Text)
                    ? "iar_document"
                    : iarNo.Text.Trim().Replace(" ", "_"));

            return $"{baseName}{extension}";
        }
    }
}