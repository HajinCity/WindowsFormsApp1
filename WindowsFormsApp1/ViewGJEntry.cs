using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class ViewGJEntry : Form
    {
        private readonly int journalId;
        private byte[] documentBytes;
        private string storedDocumentExtension;
        private Button downloadDocumentButton;
        private Label documentStatusLabel;

        public ViewGJEntry()
        {
            InitializeComponent();
            PrepareReadOnlyState();
            InitializeDocumentControls();
        }

        public ViewGJEntry(int journalId) : this()
        {
            this.journalId = journalId;
            if (journalId > 0)
            {
                LoadJournalDetails();
            }
        }

        private void PrepareReadOnlyState()
        {
            foreach (Control control in this.Controls)
            {
                SetControlReadOnly(control);
            }

            uacs_Code.DropDownStyle = ComboBoxStyle.DropDownList;
            uacs_Code.Enabled = false;
            date.Enabled = false;
        }

        private void SetControlReadOnly(Control control)
        {
            if (control is TextBox textBox)
            {
                textBox.ReadOnly = true;
                textBox.BorderStyle = BorderStyle.None;
            }

            foreach (Control child in control.Controls)
            {
                SetControlReadOnly(child);
            }
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

        private void Panel1_Resize(object sender, EventArgs e)
        {
            PositionDocumentControls();
        }

        private void PositionDocumentControls()
        {
            if (downloadDocumentButton != null)
            {
                downloadDocumentButton.Location = new Point(
                    Math.Max(0, (panel1.Width - downloadDocumentButton.Width) / 2),
                    20);
            }

            if (documentStatusLabel != null)
            {
                documentStatusLabel.Location = new Point(
                    Math.Max(0, (panel1.Width - documentStatusLabel.Width) / 2),
                    70);
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
            bool hasDocument = documentBytes != null && documentBytes.Length > 0;

            pictureBox1.Visible = !hasDocument;
            label10.Text = hasDocument ? "" : "";
            label11.Text = hasDocument ? "" : "";

            downloadDocumentButton.Visible = hasDocument;
            documentStatusLabel.Visible = hasDocument;

            if (hasDocument)
            {
                string descriptor = storedDocumentExtension;
                if (string.IsNullOrEmpty(descriptor))
                {
                    descriptor = GuessFileExtension(documentBytes);
                }
                documentStatusLabel.Text = string.IsNullOrEmpty(descriptor)
                    ? "Document available. Click below to download."
                    : $"Document available ({descriptor}). Click below to download.";
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
            string extension = storedDocumentExtension;
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

            string baseName = string.IsNullOrWhiteSpace(gjno.Text)
                ? "journal_document"
                : gjno.Text.Trim().Replace(" ", "_");

            return $"{baseName}{extension}";
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
