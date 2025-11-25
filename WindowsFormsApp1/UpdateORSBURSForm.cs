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
    public partial class UpdateORSBURSForm : Form
    {
        private readonly int orsBursId;
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

        public UpdateORSBURSForm()
        {
            InitializeComponent();
            InitializeFileUpload();
            InitializeDocumentControls();
            createORSBURSEntryBtn.Click += CreateORSBURSEntryBtn_Click;
        }

        public UpdateORSBURSForm(int orsBursId) : this()
        {
            this.orsBursId = orsBursId;
            isEditMode = orsBursId > 0;
            if (isEditMode)
            {
                LoadOrsBursDetails();
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

            Label fileType = new Label
            {
                Text = file.Extension.ToUpper().TrimStart('.'),
                Location = new Point(420, (panelHeight - 20) / 2),
                Size = new Size(60, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(128, 128, 128),
                TextAlign = ContentAlignment.MiddleLeft
            };
            itemPanel.Controls.Add(fileType);

            Label fileSize = new Label
            {
                Text = FormatFileSize(file.Length),
                Location = new Point(490, (panelHeight - 20) / 2),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(128, 128, 128),
                TextAlign = ContentAlignment.MiddleLeft
            };
            itemPanel.Controls.Add(fileSize);

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

        private void LoadOrsBursDetails()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT serial_no, date, fund_cluster, payee, office, address,
                                            responsibility_center, particulars, mfo_pap, uacs_oc,
                                            amount, approving_officer, remarks, documents
                                     FROM ora_burono
                                     WHERE ora_burono = @orsBursId
                                     LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@orsBursId", orsBursId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("ORS-BURS record could not be found.", "Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            serialNo.Text = reader["serial_no"]?.ToString();
                            Payee.Text = reader["payee"]?.ToString();
                            Office.Text = reader["office"]?.ToString();
                            FundCluster.Text = reader["fund_cluster"]?.ToString();
                            Address.Text = reader["address"]?.ToString();
                            ResponsibilityCenter.Text = reader["responsibility_center"]?.ToString();
                            Particulars.Text = reader["particulars"]?.ToString();
                            MFOPAP.Text = reader["mfo_pap"]?.ToString();
                            uacscode.Text = reader["uacs_oc"]?.ToString();
                            amount.Text = reader["amount"]?.ToString();
                            approvingOfficer.Text = reader["approving_officer"]?.ToString();
                            remarks.Text = reader["remarks"]?.ToString();

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
                        }
                    }
                }

                UpdateDocumentSection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load ORS-BURS information: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void CreateORSBURSEntryBtn_Click(object sender, EventArgs e)
        {
            if (!AreInputsValid(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to update this ORS/BURS entry?",
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
                MessageBox.Show("ORS/BURS entry updated successfully.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to update ORS/BURS entry: {ex.Message}",
                    "Update Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool AreInputsValid(out string message)
        {
            var requiredFields = new List<(string Value, string Label)>
            {
                (serialNo.Text, "Serial Number"),
                (Payee.Text, "Payee"),
                (Office.Text, "Office"),
                (FundCluster.Text, "Fund Cluster"),
                (Address.Text, "Address"),
                (ResponsibilityCenter.Text, "Responsibility Center"),
                (Particulars.Text, "Particulars"),
                (MFOPAP.Text, "MFO/PAP"),
                (uacscode.Text, "UACS Code"),
                (amount.Text, "Amount"),
                (approvingOfficer.Text, "Approving Officer"),
                (remarks.Text, "Remarks")
            };

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    message = $"{field.Label} is required.";
                    return false;
                }
            }

            if (!decimal.TryParse(amount.Text, out _))
            {
                message = "Amount must be a valid number.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        private void SaveUpdates()
        {
            byte[] attachment = documentBytes;
            decimal amountValue = decimal.Parse(amount.Text);

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            {
                string query = @"UPDATE ora_burono SET
                                    serial_no = @serial_no,
                                    date = @date,
                                    fund_cluster = @fund_cluster,
                                    payee = @payee,
                                    office = @office,
                                    address = @address,
                                    responsibility_center = @responsibility_center,
                                    particulars = @particulars,
                                    mfo_pap = @mfo_pap,
                                    uacs_oc = @uacs_oc,
                                    amount = @amount,
                                    approving_officer = @approving_officer,
                                    remarks = @remarks,
                                    documents = @documents
                                 WHERE ora_burono = @ora_burono";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@serial_no", serialNo.Text.Trim());
                    command.Parameters.AddWithValue("@date", date.Value.Date);
                    command.Parameters.AddWithValue("@fund_cluster", FundCluster.Text.Trim());
                    command.Parameters.AddWithValue("@payee", Payee.Text.Trim());
                    command.Parameters.AddWithValue("@office", Office.Text.Trim());
                    command.Parameters.AddWithValue("@address", Address.Text.Trim());
                    command.Parameters.AddWithValue("@responsibility_center", ResponsibilityCenter.Text.Trim());
                    command.Parameters.AddWithValue("@particulars", Particulars.Text.Trim());
                    command.Parameters.AddWithValue("@mfo_pap", MFOPAP.Text.Trim());
                    command.Parameters.AddWithValue("@uacs_oc", uacscode.Text.Trim());
                    command.Parameters.AddWithValue("@amount", amountValue);
                    command.Parameters.AddWithValue("@approving_officer", approvingOfficer.Text.Trim());
                    command.Parameters.AddWithValue("@remarks", remarks.Text.Trim());
                    command.Parameters.AddWithValue("@ora_burono", orsBursId);

                    var documentParam = command.Parameters.Add("@documents", MySqlDbType.LongBlob);
                    documentParam.Value = (attachment == null || attachment.Length == 0) ? (object)DBNull.Value : attachment;

                    command.ExecuteNonQuery();
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
                : (string.IsNullOrWhiteSpace(serialNo.Text)
                    ? "ors_burs_document"
                    : serialNo.Text.Trim().Replace(" ", "_"));

            return $"{baseName}{extension}";
        }
    }
}
