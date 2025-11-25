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
    public partial class AddNewJournalEntry : Form
    {
        // File upload fields
        private List<FileInfo> uploadedFiles = new List<FileInfo>();
        private Panel fileListPanel;
        private Color originalPanelColor;
        private const long MAX_FILE_SIZE = 20 * 1024 * 1024; // 20 MB in bytes
        private readonly string[] allowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
        
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
                AutoSize = true,
                AutoScroll = true,
                Visible = false
            };
            this.Controls.Add(fileListPanel);
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
            // Clear existing file items
            fileListPanel.Controls.Clear();

            if (uploadedFiles.Count == 0)
            {
                // No files uploaded - hide file list panel and show panel1
                fileListPanel.Visible = false;
                panel1.Visible = true;
                panel1.Location = originalPanel1Location;
                panel1.Size = originalPanel1Size;
                return;
            }

            // Files are uploaded - hide panel1 and show file list panel in its place
            panel1.Visible = false;
            fileListPanel.Visible = true;
            fileListPanel.Location = originalPanel1Location;
            fileListPanel.Width = originalPanel1Size.Width;
            
            int yPosition = 0;
            int itemHeight = 50;
            int spacing = 5;

            foreach (FileInfo file in uploadedFiles)
            {
                Panel fileItemPanel = CreateFileItemPanel(file, yPosition);
                fileListPanel.Controls.Add(fileItemPanel);
                yPosition += itemHeight + spacing;
            }

            // Calculate maximum height to prevent overlapping with buttons
            // Buttons are at originalButton1Top, so we have space from panel1 location to button top
            int maxAvailableHeight = originalButton1Top - fileListPanel.Top - 20; // 20px padding before buttons
            
            // Update file list panel height (limit to available space)
            fileListPanel.Height = Math.Min(yPosition, Math.Max(50, maxAvailableHeight));
            
            // Ensure buttons stay in their original positions
            createEntryBtn.Top = originalButton1Top;
            cancel.Top = originalButton2Top;
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

            // File name label
            Label fileNameLabel = new Label
            {
                Text = file.Name,
                Location = new Point(10, 15),
                Size = new Size(300, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Black,
                AutoSize = false
            };
            itemPanel.Controls.Add(fileNameLabel);

            // File type label
            string fileType = file.Extension.ToUpper().Replace(".", "");
            Label fileTypeLabel = new Label
            {
                Text = fileType,
                Location = new Point(320, 15),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };
            itemPanel.Controls.Add(fileTypeLabel);

            // File size label
            string fileSize = FormatFileSize(file.Length);
            Label fileSizeLabel = new Label
            {
                Text = fileSize,
                Location = new Point(410, 15),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleLeft
            };
            itemPanel.Controls.Add(fileSizeLabel);

            // Remove button
            Button removeButton = new Button
            {
                Text = "Remove",
                Location = new Point(600, 10),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(220, 53, 69), // Red color
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold),
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
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void RemoveFile(FileInfo file)
        {
            uploadedFiles.Remove(file);
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
                    command.Parameters.AddWithValue("@amount", amount.Text.Trim());
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
    }
}
