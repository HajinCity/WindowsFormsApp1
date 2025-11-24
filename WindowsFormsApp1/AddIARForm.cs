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
                Visible = false
            };

            Controls.Add(fileListPanel);
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
            Point scrollPosition = AutoScrollPosition;

            fileListPanel.Controls.Clear();

            if (uploadedFiles.Count == 0)
            {
                fileListPanel.Visible = false;
                panel1.Visible = true;
                panel1.Location = originalPanel1Location;
                panel1.Size = originalPanel1Size;
                AutoScrollPosition = new Point(Math.Abs(scrollPosition.X), Math.Abs(scrollPosition.Y));
                return;
            }

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

            int maxAvailableHeight = originalCreateButtonTop - fileListPanel.Top - 20;
            fileListPanel.Height = Math.Min(yPosition, Math.Max(50, maxAvailableHeight));

            createIAREntryBtn.Top = originalCreateButtonTop;
            cancel.Top = originalCancelButtonTop;

            AutoScrollPosition = new Point(Math.Abs(scrollPosition.X), Math.Abs(scrollPosition.Y));
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
                Text = file.Extension.ToUpper().TrimStart('.'),
                Location = new Point(320, 15),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
                ForeColor = Color.Gray
            };
            itemPanel.Controls.Add(fileTypeLabel);

            Label fileSizeLabel = new Label
            {
                Text = FormatFileSize(file.Length),
                Location = new Point(410, 15),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10, FontStyle.Regular),
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
                Font = new Font("Arial", 9, FontStyle.Bold),
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
    }
}
