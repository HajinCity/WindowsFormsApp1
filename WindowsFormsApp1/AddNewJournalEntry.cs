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
        
        // ComboBox filtering fields
        private List<string> originalComboBoxItems = new List<string>();
        private bool isFiltering = false;
        private Timer dropdownTimer;

        public AddNewJournalEntry()
        {
            InitializeComponent();
            InitializeFileUpload();
            InitializeComboBoxFiltering();
        }

        private void AddNewJournalEntry_Load(object sender, EventArgs e)
        {

        }
        // ============================================================
        //  COMBOBOX FILTERING FUNCTIONALITY
        // ============================================================
        private void InitializeComboBoxFiltering()
        {
            // Store original items
            originalComboBoxItems.Clear();
            foreach (object item in comboBox1.Items)
            {
                originalComboBoxItems.Add(item.ToString());
            }

            // Enable typing in ComboBox
            comboBox1.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox1.AutoCompleteMode = AutoCompleteMode.None; // We'll handle filtering manually

            // Initialize timer for opening dropdown
            dropdownTimer = new Timer();
            dropdownTimer.Interval = 50; // 50ms delay
            dropdownTimer.Tick += DropdownTimer_Tick;

            // Attach event handlers
            comboBox1.TextChanged += ComboBox1_TextChanged;
            comboBox1.KeyDown += ComboBox1_KeyDown;
            comboBox1.KeyUp += ComboBox1_KeyUp;
            comboBox1.DropDown += ComboBox1_DropDown;
            comboBox1.SelectionChangeCommitted += ComboBox1_SelectionChangeCommitted;
            comboBox1.Enter += ComboBox1_Enter;
        }

        private void DropdownTimer_Tick(object sender, EventArgs e)
        {
            dropdownTimer.Stop();
            if (!string.IsNullOrWhiteSpace(comboBox1.Text) && !comboBox1.DroppedDown)
            {
                try
                {
                    comboBox1.DroppedDown = true;
                    // Set cursor position after opening
                    comboBox1.SelectionStart = comboBox1.Text.Length;
                    comboBox1.SelectionLength = 0;
                }
                catch { }
            }
        }

        private void ComboBox1_TextChanged(object sender, EventArgs e)
        {
            if (isFiltering) return;

            string searchText = comboBox1.Text;
            isFiltering = true;

            // Clear current items
            comboBox1.Items.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // If search is empty, show all items
                comboBox1.Items.AddRange(originalComboBoxItems.ToArray());
                // Close dropdown when text is cleared
                comboBox1.DroppedDown = false;
            }
            else
            {
                // Filter items that contain the search text (case-insensitive)
                var filteredItems = originalComboBoxItems
                    .Where(item => item.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();

                comboBox1.Items.AddRange(filteredItems);

                // Always open dropdown when user is typing (even if no matches)
                // Use BeginInvoke to ensure it happens after the current event processing
                this.BeginInvoke(new Action(() =>
                {
                    if (!string.IsNullOrWhiteSpace(comboBox1.Text) && !comboBox1.DroppedDown)
                    {
                        try
                        {
                            comboBox1.DroppedDown = true;
                        }
                        catch
                        {
                            // If BeginInvoke fails, use timer as fallback
                            dropdownTimer.Stop();
                            dropdownTimer.Start();
                        }
                    }
                }));

                // Also use timer as backup to ensure it opens
                dropdownTimer.Stop();
                dropdownTimer.Start();

                // Set cursor position to end of typed text
                this.BeginInvoke(new Action(() =>
                {
                    comboBox1.SelectionStart = comboBox1.Text.Length;
                    comboBox1.SelectionLength = 0;
                }));
            }

            isFiltering = false;
        }

        private void ComboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // Allow navigation keys to work normally
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.PageUp || e.KeyCode == Keys.PageDown ||
                e.KeyCode == Keys.Home || e.KeyCode == Keys.End)
            {
                return;
            }

            // If Enter is pressed and dropdown is open, select the first item
            if (e.KeyCode == Keys.Enter && comboBox1.DroppedDown)
            {
                if (comboBox1.Items.Count > 0)
                {
                    comboBox1.SelectedIndex = 0;
                    comboBox1.DroppedDown = false;
                    e.Handled = true;
                }
            }

            // If Escape is pressed, close dropdown
            if (e.KeyCode == Keys.Escape && comboBox1.DroppedDown)
            {
                comboBox1.DroppedDown = false;
                e.Handled = true;
            }

            // For regular typing keys, ensure dropdown opens immediately
            // This handles the case when user starts typing after clicking
            bool isTypingKey = (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z) ||
                              (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                              (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) ||
                              e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete ||
                              e.KeyCode == Keys.Space;

            if (isTypingKey && !comboBox1.DroppedDown)
            {
                // Start timer to open dropdown after text updates
                dropdownTimer.Stop();
                dropdownTimer.Start();
            }
        }

        private void ComboBox1_DropDown(object sender, EventArgs e)
        {
            // When dropdown opens, ensure all items are available for selection
            if (string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                isFiltering = true;
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(originalComboBoxItems.ToArray());
                isFiltering = false;
            }
        }

        private void ComboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            // When user selects an item, update the text
            if (comboBox1.SelectedIndex >= 0)
            {
                isFiltering = true;
                comboBox1.Text = comboBox1.SelectedItem.ToString();
                isFiltering = false;
                comboBox1.DroppedDown = false;
            }
        }

        private void ComboBox1_KeyUp(object sender, KeyEventArgs e)
        {
            // Ensure dropdown opens after key is released (handles typing)
            if (!string.IsNullOrWhiteSpace(comboBox1.Text) && !comboBox1.DroppedDown)
            {
                // Use timer to open dropdown
                dropdownTimer.Stop();
                dropdownTimer.Start();
            }
        }

        private void ComboBox1_Enter(object sender, EventArgs e)
        {
            // When ComboBox gets focus, if there's text, open dropdown
            if (!string.IsNullOrWhiteSpace(comboBox1.Text))
            {
                dropdownTimer.Stop();
                dropdownTimer.Start();
            }
        }

        private void InitializeFileUpload()
        {
            // Store original panel color
            originalPanelColor = panel1.BackColor;
            
            // Store original button positions
            originalButton1Top = customRoundedButton1.Top;
            originalButton2Top = customRoundedButton2.Top;
            
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
            customRoundedButton1.Top = originalButton1Top;
            customRoundedButton2.Top = originalButton2Top;
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
    }
}
