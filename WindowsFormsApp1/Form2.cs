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
        // Form instances for embedding in panel3
        private Dashboard dashboardForm;
        private SupplierForm supplierForm;
        private GeneralJournal generalJournalForm;
        private IARForm iarForm;
        private bool shouldLoadDashboardOnShow = false;

        public Form2()
        {
            InitializeComponent();
            DashboardBtn.Click += DashboardBtn_Click;
            SupplierBtn.Click += SupplierBtn_Click;
            GJBtn.Click += GJBtn_Click;
            IARBtn.Click += IARBtn_Click;
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

                // Dispose existing forms
                DisposeForm(supplierForm);
                supplierForm = null;
                DisposeForm(generalJournalForm);
                generalJournalForm = null;
                DisposeForm(iarForm);
                iarForm = null;

                // Dispose existing dashboard if it exists
                DisposeForm(dashboardForm);
                dashboardForm = null;

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

        private void SupplierBtn_Click(object sender, EventArgs e)
        {
            LoadSupplierForm();
        }

        public void LoadSupplierForm()
        {
            try
            {
                // Clear panel3 of any existing controls
                panel3.Controls.Clear();

                // Dispose existing forms
                DisposeForm(dashboardForm);
                dashboardForm = null;
                DisposeForm(generalJournalForm);
                generalJournalForm = null;
                DisposeForm(iarForm);
                iarForm = null;

                // Create and configure SupplierForm
                supplierForm = new SupplierForm();
                supplierForm.TopLevel = false;
                supplierForm.FormBorderStyle = FormBorderStyle.None;
                supplierForm.Dock = DockStyle.Fill;
                supplierForm.Visible = true;

                // Add SupplierForm to panel3
                panel3.Controls.Add(supplierForm);
                supplierForm.Show();

                // Refresh panel to ensure proper display
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Supplier Form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GJBtn_Click(object sender, EventArgs e)
        {
            LoadGeneralJournal();
        }

        public void LoadGeneralJournal()
        {
            try
            {
                // Clear panel3 of any existing controls
                panel3.Controls.Clear();

                // Dispose existing forms
                DisposeForm(dashboardForm);
                dashboardForm = null;
                DisposeForm(supplierForm);
                supplierForm = null;
                DisposeForm(iarForm);
                iarForm = null;

                // Create and configure GeneralJournal form
                generalJournalForm = new GeneralJournal();
                generalJournalForm.TopLevel = false;
                generalJournalForm.FormBorderStyle = FormBorderStyle.None;
                generalJournalForm.Dock = DockStyle.Fill;
                generalJournalForm.Visible = true;

                // Add GeneralJournal to panel3
                panel3.Controls.Add(generalJournalForm);
                generalJournalForm.Show();

                // Refresh panel to ensure proper display
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading General Journal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void IARBtn_Click(object sender, EventArgs e)
        {
            LoadIARForm();
        }

        public void LoadIARForm()
        {
            try
            {
                // Clear panel3 of any existing controls
                panel3.Controls.Clear();

                // Dispose existing forms
                DisposeForm(dashboardForm);
                dashboardForm = null;
                DisposeForm(supplierForm);
                supplierForm = null;
                DisposeForm(generalJournalForm);
                generalJournalForm = null;

                // Create and configure IARForm
                iarForm = new IARForm();
                iarForm.TopLevel = false;
                iarForm.FormBorderStyle = FormBorderStyle.None;
                iarForm.Dock = DockStyle.Fill;
                iarForm.Visible = true;

                // Add IARForm to panel3
                panel3.Controls.Add(iarForm);
                iarForm.Show();

                // Refresh panel to ensure proper display
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading IAR Form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Helper method to dispose forms safely
        private void DisposeForm(Form form)
        {
            if (form != null)
            {
                form.Hide();
                form.Dispose();
            }
        }
    }
}
