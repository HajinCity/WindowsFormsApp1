using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class UpdateGJ : Form
    {
        private readonly int journalId;
        private readonly bool isEditMode;

        private byte[] documentBytes;
        private byte[] originalDocumentBytes;
        private string storedDocumentExtension;
        private string uploadedDocumentName;
        private string uploadedDocumentExtension;

        private Button downloadDocumentButton;
        private Button changeDocumentButton;
        private Button removeDocumentButton;
        private Label documentStatusLabel;

        private readonly List<FileInfo> uploadedFiles = new List<FileInfo>();
        private Panel fileListPanel;
        private Color originalPanelColor;
        private Point originalPanel1Location;
        private Size originalPanel1Size;
        private const long MAX_FILE_SIZE = 20 * 1024 * 1024; // 20 MB
        private readonly string[] allowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
        private bool isFormattingAmountText;

        public UpdateGJ()
        {
            InitializeComponent();
            InitializeDocumentControls();
            InitializeFileUpload();
            createUpdateEntryBtn.Click += CreateUpdateEntryBtn_Click;
            cancel.Click += (s, e) => this.Close();
            amount.TextChanged += Amount_TextChanged;
        }

        public UpdateGJ(int journalId) : this()
        {
            this.journalId = journalId;
            isEditMode = journalId > 0;
            if (isEditMode)
            {
                LoadJournalDetails();
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

            foreach (Control control in new Control[] { pictureBox1, label10, label11 })
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
            this.Controls.Add(fileListPanel);
            fileListPanel.BringToFront();
        }

        private void Panel1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Supported Files|*.pdf;*.png;*.jpg;*.jpeg;*.docx|All Files|*.*";
                openFileDialog.Multiselect = false;
                openFileDialog.Title = "Select Document to Upload";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessFiles(openFileDialog.FileNames);
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
                    MessageBox.Show("Only one file can be uploaded at a time. The first file will be used.",
                        "Multiple Files", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                ProcessFiles(new string[] { files[0] });
            }
        }

        private void ProcessFiles(string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
                return;

            string filePath = filePaths[0];

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);

                string extension = fileInfo.Extension.ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    MessageBox.Show($"File '{fileInfo.Name}' has an unsupported file type.",
                        "Invalid File Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (fileInfo.Length > MAX_FILE_SIZE)
                {
                    MessageBox.Show($"File '{fileInfo.Name}' exceeds the maximum file size of 20 MB.",
                        "File Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte[] fileBytes = File.ReadAllBytes(filePath);
                documentBytes = fileBytes;
                uploadedDocumentName = fileInfo.Name;
                uploadedDocumentExtension = fileInfo.Extension;
                storedDocumentExtension = uploadedDocumentExtension;

                uploadedFiles.Clear();
                uploadedFiles.Add(fileInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing file '{filePath}': {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateDocumentSection();
        }

        private void UpdateFileList()
        {
            fileListPanel.Controls.Clear();

            if (uploadedFiles.Count == 0)
            {
                fileListPanel.Visible = false;
                panel1.Visible = true;
                return;
            }

            panel1.Visible = false;
            fileListPanel.Visible = true;
            fileListPanel.Location = originalPanel1Location;
            fileListPanel.Size = originalPanel1Size;

            int yPosition = 0;
            foreach (var file in uploadedFiles)
            {
                Panel fileItemPanel = CreateFileItemPanel(file, yPosition);
                fileListPanel.Controls.Add(fileItemPanel);
                yPosition += fileItemPanel.Height + 5;
            }
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

            Label fileTypeLabel = new Label
            {
                Text = file.Extension.ToUpper().Trim('.'),
                Location = new Point(320, 15),
                Size = new Size(80, 20),
                ForeColor = Color.Gray
            };
            itemPanel.Controls.Add(fileTypeLabel);

            Label fileSizeLabel = new Label
            {
                Text = FormatFileSize(file.Length),
                Location = new Point(410, 15),
                Size = new Size(100, 20),
                ForeColor = Color.Gray
            };
            itemPanel.Controls.Add(fileSizeLabel);

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

        private void RemoveFile(FileInfo file)
        {
            uploadedFiles.Remove(file);
            if (uploadedFiles.Count == 0)
            {
                uploadedDocumentName = null;
                uploadedDocumentExtension = null;
                documentBytes = originalDocumentBytes;
                storedDocumentExtension = GuessFileExtension(originalDocumentBytes);
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

        private void LoadJournalDetails()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT gj_no, particulars, uacs_code, amount, date, documents
                                     FROM general_journal
                                     WHERE gj_id = @gjId
                                     LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@gjId", journalId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Journal entry could not be found.", "Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            gjno.Text = reader["gj_no"]?.ToString();
                            particulars.Text = reader["particulars"]?.ToString();
                            uacs_Code.Text = reader["uacs_code"]?.ToString();
                            amount.Text = reader["amount"]?.ToString();
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
                            originalDocumentBytes = documentBytes;
                            storedDocumentExtension = GuessFileExtension(documentBytes);

                            UpdateDocumentSection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load journal entry: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateDocumentSection()
        {
            bool hasNewUpload = uploadedFiles.Count > 0;
            bool hasDocument = documentBytes != null && documentBytes.Length > 0;

            if (hasNewUpload)
            {
                UpdateFileList();
                fileListPanel.Visible = true;
                panel1.Visible = false;
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
                label10.Text = "";
                label11.Text = "";

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
                label10.Text = "Click to upload or drag and drop";
                label11.Text = "PDF,JPG,PNG,DOCX (20MB)";

                downloadDocumentButton.Visible = false;
                changeDocumentButton.Visible = false;
                removeDocumentButton.Visible = false;
                documentStatusLabel.Visible = false;
            }

            PositionDocumentControls();
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
                saveFileDialog.Title = "Save Journal Document";
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
                        var promptResult = MessageBox.Show(
                            "Document saved successfully. Do you want to open it now?",
                            "Document Saved",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (promptResult == DialogResult.Yes)
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

        private void CreateUpdateEntryBtn_Click(object sender, EventArgs e)
        {
            if (!AreRequiredFieldsFilled(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to update this journal entry?",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                UpdateJournalRecord();
                MessageBox.Show("Journal entry has been successfully updated.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to update journal entry: {ex.Message}",
                    "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool AreRequiredFieldsFilled(out string message)
        {
            var requiredFields = new List<(string Value, string Label)>
            {
                (gjno.Text, "GJ Number"),
                (uacs_Code.Text, "UACS Code"),
                (amount.Text, "Amount"),
                (particulars.Text, "Particulars")
            };

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    message = $"{field.Label} is required.";
                    return false;
                }
            }

            if (!TryGetAmountValue(out _))
            {
                message = "Amount must be a valid number.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private byte[] GetDocumentBytes()
        {
            if (uploadedFiles.Count == 0)
            {
                return documentBytes;
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

        private void UpdateJournalRecord()
        {
            if (!isEditMode)
            {
                throw new InvalidOperationException("No journal entry selected to update.");
            }

            byte[] attachment = GetDocumentBytes();
            if (!TryGetAmountValue(out decimal amountValue))
            {
                throw new InvalidOperationException("Unable to parse the Amount field.");
            }

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            {
                string query = @"UPDATE general_journal SET 
                                    gj_no = @gj_no,
                                    particulars = @particulars,
                                    uacs_code = @uacs_code,
                                    amount = @amount,
                                    date = @date,
                                    documents = @documents
                                 WHERE gj_id = @gjId";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@gj_no", gjno.Text.Trim());
                    command.Parameters.AddWithValue("@particulars", particulars.Text.Trim());
                    command.Parameters.AddWithValue("@uacs_code", uacs_Code.Text.Trim());
                    command.Parameters.AddWithValue("@amount", amountValue);
                    command.Parameters.AddWithValue("@date", date.Value.Date);
                    command.Parameters.AddWithValue("@gjId", journalId);

                    var documentParam = command.Parameters.Add("@documents", MySqlDbType.LongBlob);
                    if (attachment == null || attachment.Length == 0)
                    {
                        documentParam.Value = DBNull.Value;
                    }
                    else
                    {
                        documentParam.Value = attachment;
                    }

                    command.ExecuteNonQuery();
                }
            }
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
                : (string.IsNullOrWhiteSpace(gjno.Text)
                    ? "journal_document"
                    : gjno.Text.Trim().Replace(" ", "_"));

            return $"{baseName}{extension}";
        }

        private bool TryGetAmountValue(out decimal amountValue)
        {
            string numericText = amount.Text?.Replace(",", "").Trim();
            if (string.IsNullOrWhiteSpace(numericText))
            {
                amountValue = 0m;
                return false;
            }

            return decimal.TryParse(
                numericText,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out amountValue);
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
            int selectionFromEnd = currentText.Length - amount.SelectionStart;
            amount.Text = formattedText;
            int newSelectionStart = formattedText.Length - selectionFromEnd;
            if (newSelectionStart < 0)
            {
                newSelectionStart = 0;
            }
            amount.SelectionStart = Math.Min(newSelectionStart, amount.Text.Length);
            amount.SelectionLength = 0;
            isFormattingAmountText = false;
        }
    }
}
