using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using WindowsFormsApp1.BackendModel;

namespace WindowsFormsApp1
{
    public partial class ViewSupplier : Form
    {
        private readonly int supplierId;
        private readonly int loggedInUserId;
        private byte[] documentBytes;
        private string storedDocumentExtension;
        private Button downloadDocumentButton;
        private Label documentStatusLabel;

        public ViewSupplier()
        {
            InitializeComponent();
            InitializeStatusDropdown();
            InitializeDocumentControls();
            MakeFieldsReadOnly();
        }

        public ViewSupplier(int supplierId, int userId) : this()
        {
            this.supplierId = supplierId;
            this.loggedInUserId = userId;
            LoadSupplierDetails();
        }

        private void InitializeStatusDropdown()
        {
            status.Items.Clear();
            status.Items.AddRange(new object[] { "Active", "Inactive" });
            status.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void MakeFieldsReadOnly()
        {
            foreach (Control control in this.Controls)
            {
                SetReadOnly(control);
            }
        }

        private void SetReadOnly(Control control)
        {
            if (control is TextBox textBox)
            {
                textBox.ReadOnly = true;
                textBox.BorderStyle = BorderStyle.None;
            }

            foreach (Control child in control.Controls)
            {
                SetReadOnly(child);
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
                Text = "A document is attached to this supplier.",
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
                    25);
            }

            if (documentStatusLabel != null)
            {
                documentStatusLabel.Location = new Point(
                    Math.Max(0, (panel1.Width - documentStatusLabel.Width) / 2),
                    75);
            }
        }

        private void LoadSupplierDetails()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"SELECT supplier_id, name, address, contact_person, contact_number, email,
                                            tin, bank_name, account_number, status, documents
                                     FROM suppliers
                                     WHERE supplier_id = @supplierId
                                     LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@supplierId", supplierId);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Supplier record could not be found.", "Not Found",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            supplierName.Text = reader["name"]?.ToString();
                            supplierAddress.Text = reader["address"]?.ToString();
                            contactPerson.Text = reader["contact_person"]?.ToString();
                            contactInfo.Text = reader["contact_number"]?.ToString();
                            emailAdd.Text = reader["email"]?.ToString();
                            tinNumber.Text = reader["tin"]?.ToString();
                            bankName.Text = reader["bank_name"]?.ToString();
                            accountNum.Text = reader["account_number"]?.ToString();
                            status.Text = reader["status"]?.ToString();

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
                            
                            // Log user activity
                            LogUserActivity(
                                loggedInUserId,
                                "Viewed",
                                "Supplier Management",
                                $"Viewed supplier: {supplierName.Text}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load supplier information: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void UpdateDocumentSection()
        {
            bool hasDocument = documentBytes != null && documentBytes.Length > 0;

            pictureBox1.Visible = !hasDocument;
            label10.Visible = !hasDocument;
            label11.Visible = !hasDocument;

            downloadDocumentButton.Visible = hasDocument;
            documentStatusLabel.Visible = hasDocument;

            if (hasDocument)
            {
                string displayExtension = storedDocumentExtension;
                if (string.IsNullOrEmpty(displayExtension))
                {
                    displayExtension = GuessFileExtension(documentBytes);
                }
                documentStatusLabel.Text = string.IsNullOrEmpty(displayExtension)
                    ? "Document available. Click the button below to download."
                    : $"Document available ({displayExtension}). Click the button below to download.";
            }

            PositionDocumentControls();
        }

        private void DownloadDocumentButton_Click(object sender, EventArgs e)
        {
            if (documentBytes == null || documentBytes.Length == 0)
            {
                MessageBox.Show("No document is available for this supplier.", "No Document",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Save Supplier Document";
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

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.Close();
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

            string baseName = string.IsNullOrWhiteSpace(supplierName.Text)
                ? "supplier_document"
                : supplierName.Text.Trim().Replace(" ", "_");

            return $"{baseName}{extension}";
        }

        private void LogUserActivity(int userId, string action, string module, string details)
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string query = @"
                        INSERT INTO userlogs (user_id, users, action, module, details, ip_address, action_timestamp)
                        SELECT 
                            u.user_id,
                            u.full_name,
                            @action,
                            @module,
                            @details,
                            @ip_address,
                            NOW()
                        FROM users u
                        WHERE u.user_id = @user_id;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@user_id", userId);
                        command.Parameters.AddWithValue("@action", action);
                        command.Parameters.AddWithValue("@module", module);
                        command.Parameters.AddWithValue("@details", details ?? string.Empty);
                        command.Parameters.AddWithValue("@ip_address", GetLocalIpAddress());

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't block view operation
                System.Diagnostics.Debug.WriteLine($"Failed to log user activity: {ex.Message}");
            }
        }

        private string GetLocalIpAddress()
        {
            try
            {
                string localIP = "";
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }

                return string.IsNullOrEmpty(localIP) ? "Unknown" : localIP;
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
