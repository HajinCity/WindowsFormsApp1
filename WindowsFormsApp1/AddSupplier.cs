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

namespace WindowsFormsApp1
{
    public partial class AddSupplier : Form
    {
        private List<FileInfo> uploadedFiles = new List<FileInfo>();
        private Panel fileListPanel;
        private Color originalPanelColor;
        private const long MAX_FILE_SIZE = 20 * 1024 * 1024; // 20 MB in bytes
        private readonly string[] allowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };

        public AddSupplier()
        {
            InitializeComponent();
            InitializeFileUpload();
        }

        private void InitializeFileUpload()
        {
            // Store original panel color
            originalPanelColor = panel1.BackColor;

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

            // Create file list panel below upload area
            fileListPanel = new Panel
            {
                Location = new Point(30, panel1.Bottom + 10),
                Size = new Size(706, 100),
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
                fileListPanel.Visible = false;
                return;
            }

            fileListPanel.Visible = true;
            int yPosition = 0;
            int itemHeight = 50;
            int spacing = 5;

            foreach (FileInfo file in uploadedFiles)
            {
                Panel fileItemPanel = CreateFileItemPanel(file, yPosition);
                fileListPanel.Controls.Add(fileItemPanel);
                yPosition += itemHeight + spacing;
            }

            // Update file list panel height
            fileListPanel.Height = Math.Min(yPosition, 200); // Max height with scroll

            // Adjust form size if needed
            int newBottom = fileListPanel.Bottom + 20;
            if (newBottom > this.Height)
            {
                this.Height = newBottom + 100; // Add some padding
            }

            // Adjust button positions
            customRoundedButton1.Top = fileListPanel.Bottom + 20;
            customRoundedButton2.Top = fileListPanel.Bottom + 20;
        }

        private Panel CreateFileItemPanel(FileInfo file, int yPosition)
        {
            Panel itemPanel = new Panel
            {
                Location = new Point(0, yPosition),
                Size = new Size(706, 50),
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

        // Public method to get uploaded files (can be used when creating supplier)
        public List<FileInfo> GetUploadedFiles()
        {
            return new List<FileInfo>(uploadedFiles);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
