using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class AddNewJournalEntry : Form
    {
        // File upload fields
        private List<FileInfo> uploadedFiles = new List<FileInfo>();
        private Panel fileListPanel;
        private Color originalPanelColor;
        private const long MAX_FILE_SIZE = 20 * 1024 * 1024; // 20 MB in bytes
        private readonly string[] allowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
        private bool isFormattingAmountText;
        
        // Store original button positions
        private int originalButton1Top;
        private int originalButton2Top;
        
        // Store original panel1 position and size
        private Point originalPanel1Location;
        private Size originalPanel1Size;
        
        public AddNewJournalEntry()
        {
            InitializeComponent();
            InitializeFileUpload();
            createEntryBtn.Click += CreateEntryBtn_Click;
            cancel.Click += (s, e) => this.Close();
            amount.TextChanged += Amount_TextChanged;
            amount.KeyPress += Amount_KeyPress;
        }

        private void AddNewJournalEntry_Load(object sender, EventArgs e)
        {

        }
        private void InitializeFileUpload()
        {
            // Store original panel color
            originalPanelColor = panel1.BackColor;
            
            // Store original button positions
            originalButton1Top = createEntryBtn.Top;
            originalButton2Top = cancel.Top;
            
            // Store original panel1 position and size
            originalPanel1Location = panel1.Location;
            originalPanel1Size = panel1.Size;

            // Enable drag and drop on panel1
            panel1.AllowDrop = true;
            panel1.DragEnter += Panel1_DragEnter;
            panel1.DragOver += Panel1_DragOver;
            panel1.DragLeave += Panel1_DragLeave;
            panel1.DragDrop += Panel1_DragDrop;
            panel1.Click += Panel1_Click;
            panel1.Cursor = Cursors.Hand;

            // Make child controls also trigger the click and support drag/drop
            foreach (Control control in panel1.Controls)
            {
                control.Click += Panel1_Click;
                control.AllowDrop = true;
                control.DragEnter += Panel1_DragEnter;
                control.DragOver += Panel1_DragOver;
                control.DragLeave += Panel1_DragLeave;
                control.DragDrop += Panel1_DragDrop;
                control.Cursor = Cursors.Hand;
            }

            // Create file list panel at the same location as panel1 (will be positioned when files are uploaded)
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

        private void Panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                panel1.BackColor = Color.FromArgb(240, 248, 255); // Light blue highlight
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
            panel1.BackColor = originalPanelColor; // Reset to original color
        }

        private void Panel1_DragDrop(object sender, DragEventArgs e)
        {
            panel1.BackColor = originalPanelColor; // Reset to original color
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                // Only process the first file if multiple files are dropped
                if (files.Length > 1)
                {
                    MessageBox.Show("Only one file can be uploaded at a time. The first file will be used.", 
                        "Multiple Files", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                ProcessFiles(new string[] { files[0] });
            }
        }

        private void Panel1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Supported Files|*.pdf;*.png;*.jpg;*.jpeg;*.docx|PDF Files|*.pdf|Image Files|*.png;*.jpg;*.jpeg|Word Documents|*.docx|All Files|*.*";
                openFileDialog.Multiselect = false;
                openFileDialog.Title = "Select Document to Upload";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ProcessFiles(openFileDialog.FileNames);
                }
            }
        }

        private void ProcessFiles(string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
                return;

            // Only process the first file
            string filePath = filePaths[0];

            try
            {
                FileInfo fileInfo = new FileInfo(filePath);

                // Validate file extension
                string extension = fileInfo.Extension.ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    MessageBox.Show($"File '{fileInfo.Name}' has an unsupported file type. Supported types: PDF, PNG, JPG, DOCX", 
                        "Invalid File Type", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate file size
                if (fileInfo.Length > MAX_FILE_SIZE)
                {
                    MessageBox.Show($"File '{fileInfo.Name}' exceeds the maximum file size of 20 MB.", 
                        "File Too Large", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Clear existing files and add the new one (only one file allowed)
                uploadedFiles.Clear();
                uploadedFiles.Add(fileInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing file '{filePath}': {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateFileList();
        }

        private void UpdateFileList()
        {
            SuspendLayout();
            
            try
            {
                // Clear existing file items
                fileListPanel.Controls.Clear();

                if (uploadedFiles.Count == 0)
                {
                    // No files uploaded - hide file list panel and show panel1
                    fileListPanel.Controls.Clear();
                    fileListPanel.Height = 0; // shrink
                    fileListPanel.Visible = false;
                    panel1.Visible = true;
                    panel1.Location = originalPanel1Location;
                    panel1.Size = originalPanel1Size;
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
                createEntryBtn.Top = originalButton1Top;
                cancel.Top = originalButton2Top;

                // Ensure it's in the correct Z-order and visible
                fileListPanel.BringToFront();
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

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
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

        // Public method to get uploaded files (can be used when creating journal entry)
        public List<FileInfo> GetUploadedFiles()
        {
            return new List<FileInfo>(uploadedFiles);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CreateEntryBtn_Click(object sender, EventArgs e)
        {
            if (!AreInputsValid(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to save this general journal entry?",
                "Confirm Entry",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                SaveJournalEntry();
                MessageBox.Show("Journal entry saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to save journal entry: {ex.Message}",
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool AreInputsValid(out string message)
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

        private void SaveJournalEntry()
        {
            byte[] documentBytes = GetDocumentBytes();
            if (!TryGetAmountValue(out decimal amountValue))
            {
                throw new InvalidOperationException("Unable to parse the Amount field.");
            }

            using (MySqlConnection connection = RDBSMConnection.GetConnection())
            {
                string query = @"INSERT INTO general_journal 
                                (gj_no, particulars, uacs_code, amount, date, documents)
                                VALUES 
                                (@gj_no, @particulars, @uacs_code, @amount, @date, @documents)";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@gj_no", gjno.Text.Trim());
                    command.Parameters.AddWithValue("@particulars", particulars.Text.Trim());
                    command.Parameters.AddWithValue("@uacs_code", uacs_Code.Text.Trim());
                    command.Parameters.AddWithValue("@amount", amountValue);
                    command.Parameters.AddWithValue("@date", date.Value.Date);

                    var documentParam = command.Parameters.Add("@documents", MySqlDbType.LongBlob);
                    if (documentBytes == null || documentBytes.Length == 0)
                    {
                        documentParam.Value = DBNull.Value;
                    }
                    else
                    {
                        documentParam.Value = documentBytes;
                    }

                    command.ExecuteNonQuery();
                }
            }
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

        private void Amount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            if (e.KeyChar == '.')
            {
                TextBox textBox = sender as TextBox;
                if (textBox != null && !textBox.Text.Contains("."))
                {
                    return;
                }
            }

            e.Handled = true;
        }
    }
}
