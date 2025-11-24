using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        // Dashboard form instance for embedding in panel3
        private Dashboard dashboardForm;
        private bool shouldLoadDashboardOnShow = false;

        public Form2()
        {
            InitializeComponent();
            DashboardBtn.Click += DashboardBtn_Click;
            this.Shown += Form2_Shown;
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            if (shouldLoadDashboardOnShow)
            {
                // Programmatically trigger DashboardBtn click to update visual state and load dashboard
                DashboardBtn.PerformClick();
                shouldLoadDashboardOnShow = false;
            }
        }

        private void DashboardBtn_Click(object sender, EventArgs e)
        {
            LoadDashboard();
        }

        public void LoadDashboard()
        {
            try
            {
                // Clear panel3 of any existing controls
                panel3.Controls.Clear();

                // Dispose existing dashboard if it exists
                if (dashboardForm != null)
                {
                    dashboardForm.Hide();
                    dashboardForm.Dispose();
                    dashboardForm = null;
                }

                // Create and configure Dashboard form
                dashboardForm = new Dashboard();
                dashboardForm.TopLevel = false;
                dashboardForm.FormBorderStyle = FormBorderStyle.None;
                dashboardForm.Dock = DockStyle.Fill;
                dashboardForm.Visible = true;

                // Add Dashboard to panel3
                panel3.Controls.Add(dashboardForm);
                dashboardForm.Show();

                // Refresh panel to ensure proper display
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Dashboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SetAutoLoadDashboard(bool autoLoad)
        {
            shouldLoadDashboardOnShow = autoLoad;
        }
    }
}
