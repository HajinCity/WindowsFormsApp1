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
    public partial class UpdateJEV : Form
    {
        private readonly int jevId;
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

        public UpdateJEV()
        {
            InitializeComponent();
            InitializeFileUpload();
            InitializeDocumentControls();
            UpdateJournalEntryBtn.Click += UpdateJournalEntryBtn_Click;
            cancel.Click += cancel_Click;
        }

        public UpdateJEV(int jevId) : this()
        {
            this.jevId = jevId;
            isEditMode = jevId > 0;
            if (isEditMode)
            {
                LoadJEVDetails();
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
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

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
                downloadDocumentButton.Visible = false;
                changeDocumentButton.Visible = false;
                removeDocumentButton.Visible = false;
                documentStatusLabel.Visible = false;
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
            SuspendLayout();
            
            try
            {
                Point currentPanelLocation = panel1.Location;
                Size currentPanelSize = panel1.Size;
                
                panel1.Visible = false;
                
                fileListPanel.Location = currentPanelLocation;
                fileListPanel.Size = new Size(currentPanelSize.Width, 0);
                fileListPanel.Visible = true;
                fileListPanel.Controls.Clear();
                fileListPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                
                int yPosition = 0;
                foreach (FileInfo file in uploadedFiles)
                {
                    Panel fileItemPanel = CreateFileItemPanel(file, yPosition);
                    fileListPanel.Controls.Add(fileItemPanel);
                    yPosition += fileItemPanel.Height + 5;
                }
                fileListPanel.Height = yPosition;
                
                fileListPanel.BringToFront();
            }
            finally
            {
                ResumeLayout(true);
                fileListPanel.Refresh();
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
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label fileNameLabel = new Label
            {
                Text = file.Name,
                Location = new Point(horizontalPadding, (panelHeight - 20) / 2),
                Size = new Size(400, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoEllipsis = true
            };
            itemPanel.Controls.Add(fileNameLabel);

            Label fileTypeLabel = new Label
            {
                Text = file.Extension.ToUpper().TrimStart('.'),
                Location = new Point(420, (panelHeight - 20) / 2),
                Size = new Size(60, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(128, 128, 128),
                TextAlign = ContentAlignment.MiddleLeft
            };
            itemPanel.Controls.Add(fileTypeLabel);

            Label fileSizeLabel = new Label
            {
                Text = FormatFileSize(file.Length),
                Location = new Point(490, (panelHeight - 20) / 2),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(128, 128, 128),
                TextAlign = ContentAlignment.MiddleLeft
            };
            itemPanel.Controls.Add(fileSizeLabel);

            Button removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(panelWidth - 100 - horizontalPadding, (panelHeight - 30) / 2),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Arial", 9, FontStyle.Regular),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
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
                fileListPanel.Height = 0;
                fileListPanel.Visible = false;
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

        private string GuessFileExtension(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4)
            {
                return ".bin";
            }

            // PDF
            if (bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46)
            {
                return ".pdf";
            }

            // PNG
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            {
                return ".png";
            }

            // JPEG
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            {
                return ".jpg";
            }

            // DOCX (ZIP-based)
            if (bytes[0] == 0x50 && bytes[1] == 0x4B && bytes[2] == 0x03 && bytes[3] == 0x04)
            {
                return ".docx";
            }

            return ".bin";
        }

        private string GetSuggestedDocumentFileName()
        {
            string baseName = $"JEV_{jevId}";
            string extension = storedDocumentExtension ?? ".bin";
            return $"{baseName}{extension}";
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
                            originalDocumentBytes = documentBytes;
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

        private void UpdateJournalEntryBtn_Click(object sender, EventArgs e)
        {
            if (!AreInputsValid(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to update this JEV entry?",
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
                MessageBox.Show("JEV entry updated successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update JEV entry: {ex.Message}",
                    "Update Error",
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

        private void SaveUpdates()
        {
            byte[] documentBytesToSave = documentBytes;
            decimal grossAmountValue = decimal.Parse(grossAmount.Text);
            decimal deductionsValue = decimal.Parse(deductions.Text);
            decimal netAmountValue = decimal.Parse(netAmount.Text);

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            {
                string query = @"UPDATE jev SET
                                    jev_no = @jev_no,
                                    date = @date,
                                    responsibility_center = @responsibility_center,
                                    uacs_code = @uacs_code,
                                    account = @account,
                                    particulars = @particulars,
                                    gross_amount = @gross_amount,
                                    deductions = @deductions,
                                    tax_type = @tax_type,
                                    net_amount = @net_amount,
                                    status = @status,
                                    approving_officer = @approving_officer,
                                    documents = @documents
                                 WHERE jev_id = @jev_id";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@jev_id", jevId);
                    command.Parameters.AddWithValue("@jev_no", jev_no.Text.Trim());
                    command.Parameters.AddWithValue("@date", jevDate.Value.Date);
                    command.Parameters.AddWithValue("@responsibility_center", rspCode.Text.Trim());
                    command.Parameters.AddWithValue("@uacs_code", uacscode.Text.Trim());
                    command.Parameters.AddWithValue("@account", account.Text.Trim());
                    command.Parameters.AddWithValue("@particulars", particulars.Text.Trim());
                    command.Parameters.AddWithValue("@gross_amount", grossAmountValue);
                    command.Parameters.AddWithValue("@deductions", deductionsValue);
                    command.Parameters.AddWithValue("@tax_type", taxtype.Text.Trim());
                    command.Parameters.AddWithValue("@net_amount", netAmountValue);
                    command.Parameters.AddWithValue("@status", status.Text.Trim());
                    command.Parameters.AddWithValue("@approving_officer", approvingOfficer.Text.Trim());

                    var documentParam = command.Parameters.Add("@documents", MySqlDbType.LongBlob);
                    if (documentBytesToSave == null || documentBytesToSave.Length == 0)
                    {
                        documentParam.Value = DBNull.Value;
                    }
                    else
                    {
                        documentParam.Value = documentBytesToSave;
                    }

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
