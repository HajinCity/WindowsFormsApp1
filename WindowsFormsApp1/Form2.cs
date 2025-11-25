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
        private OrsBursForm orsBursForm;
        private JEVForm jevForm;
        private DEVForm devForm;
        private TransactionLedger transactionLedgerForm;
        private UserManagement userManagementForm;
        private bool shouldLoadDashboardOnShow = false;

        public Form2()
        {
            InitializeComponent();
            DashboardBtn.Click += DashboardBtn_Click;
            SupplierBtn.Click += SupplierBtn_Click;
            GJBtn.Click += GJBtn_Click;
            IARBtn.Click += IARBtn_Click;
            ORSBtn.Click += ORSBtn_Click;
            JEVBtn.Click += JEVBtn_Click;
            DVBtn.Click += DVBtn_Click;
            TransactionLedger.Click += TransactionLedgerBtn_Click;
            UserManagementBtn.Click += UserManagementBtn_Click;
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
                DisposeForm(orsBursForm);
                orsBursForm = null;
                DisposeForm(jevForm);
                jevForm = null;
                DisposeForm(devForm);
                devForm = null;
                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;
                DisposeForm(userManagementForm);
                userManagementForm = null;

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
                DisposeForm(orsBursForm);
                orsBursForm = null;
                DisposeForm(jevForm);
                jevForm = null;
                DisposeForm(devForm);
                devForm = null;
                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;
                DisposeForm(userManagementForm);
                userManagementForm = null;

                DisposeForm(supplierForm);
                supplierForm = null;

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
                DisposeForm(orsBursForm);
                orsBursForm = null;
                DisposeForm(jevForm);
                jevForm = null;
                DisposeForm(devForm);
                devForm = null;
                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;
                DisposeForm(userManagementForm);
                userManagementForm = null;

                DisposeForm(generalJournalForm);
                generalJournalForm = null;

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
                DisposeForm(orsBursForm);
                orsBursForm = null;
                DisposeForm(jevForm);
                jevForm = null;
                DisposeForm(devForm);
                devForm = null;
                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;
                DisposeForm(userManagementForm);
                userManagementForm = null;

                DisposeForm(iarForm);
                iarForm = null;

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

        private void ORSBtn_Click(object sender, EventArgs e)
        {
            LoadOrsBursForm();
        }

        public void LoadOrsBursForm()
        {
            try
            {
                panel3.Controls.Clear();

                DisposeForm(dashboardForm);
                dashboardForm = null;
                DisposeForm(supplierForm);
                supplierForm = null;
                DisposeForm(generalJournalForm);
                generalJournalForm = null;
                DisposeForm(iarForm);
                iarForm = null;

                DisposeForm(orsBursForm);
                orsBursForm = null;

                DisposeForm(jevForm);
                jevForm = null;
                DisposeForm(devForm);
                devForm = null;
                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;
                DisposeForm(userManagementForm);
                userManagementForm = null;

                orsBursForm = new OrsBursForm();
                orsBursForm.TopLevel = false;
                orsBursForm.FormBorderStyle = FormBorderStyle.None;
                orsBursForm.Dock = DockStyle.Fill;
                orsBursForm.Visible = true;

                panel3.Controls.Add(orsBursForm);
                orsBursForm.Show();
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ORS-BURS Form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void JEVBtn_Click(object sender, EventArgs e)
        {
            LoadJEVForm();
        }

        public void LoadJEVForm()
        {
            try
            {
                panel3.Controls.Clear();

                DisposeForm(dashboardForm);
                dashboardForm = null;
                DisposeForm(supplierForm);
                supplierForm = null;
                DisposeForm(generalJournalForm);
                generalJournalForm = null;
                DisposeForm(iarForm);
                iarForm = null;
                DisposeForm(orsBursForm);
                orsBursForm = null;
                DisposeForm(devForm);
                devForm = null;
                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;
                DisposeForm(userManagementForm);
                userManagementForm = null;

                DisposeForm(jevForm);
                jevForm = null;

                jevForm = new JEVForm();
                jevForm.TopLevel = false;
                jevForm.FormBorderStyle = FormBorderStyle.None;
                jevForm.Dock = DockStyle.Fill;
                jevForm.Visible = true;

                panel3.Controls.Add(jevForm);
                jevForm.Show();
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading JEV Form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DVBtn_Click(object sender, EventArgs e)
        {
            LoadDEVForm();
        }

        public void LoadDEVForm()
        {
            try
            {
                panel3.Controls.Clear();

                DisposeForm(dashboardForm);
                dashboardForm = null;
                DisposeForm(supplierForm);
                supplierForm = null;
                DisposeForm(generalJournalForm);
                generalJournalForm = null;
                DisposeForm(iarForm);
                iarForm = null;
                DisposeForm(orsBursForm);
                orsBursForm = null;
                DisposeForm(jevForm);
                jevForm = null;
                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;
                DisposeForm(userManagementForm);
                userManagementForm = null;

                DisposeForm(devForm);
                devForm = null;

                devForm = new DEVForm();
                devForm.TopLevel = false;
                devForm.FormBorderStyle = FormBorderStyle.None;
                devForm.Dock = DockStyle.Fill;
                devForm.Visible = true;

                panel3.Controls.Add(devForm);
                devForm.Show();
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading DEV Form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TransactionLedgerBtn_Click(object sender, EventArgs e)
        {
            LoadTransactionLedger();
        }

        public void LoadTransactionLedger()
        {
            try
            {
                panel3.Controls.Clear();

                DisposeForm(dashboardForm);
                dashboardForm = null;
                DisposeForm(supplierForm);
                supplierForm = null;
                DisposeForm(generalJournalForm);
                generalJournalForm = null;
                DisposeForm(iarForm);
                iarForm = null;
                DisposeForm(orsBursForm);
                orsBursForm = null;
                DisposeForm(jevForm);
                jevForm = null;
                DisposeForm(devForm);
                devForm = null;
                DisposeForm(userManagementForm);
                userManagementForm = null;

                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;

                transactionLedgerForm = new TransactionLedger();
                transactionLedgerForm.TopLevel = false;
                transactionLedgerForm.FormBorderStyle = FormBorderStyle.None;
                transactionLedgerForm.Dock = DockStyle.Fill;
                transactionLedgerForm.Visible = true;

                panel3.Controls.Add(transactionLedgerForm);
                transactionLedgerForm.Show();
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Transaction Ledger: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UserManagementBtn_Click(object sender, EventArgs e)
        {
            LoadUserManagement();
        }

        public void LoadUserManagement()
        {
            try
            {
                panel3.Controls.Clear();

                DisposeForm(dashboardForm);
                dashboardForm = null;
                DisposeForm(supplierForm);
                supplierForm = null;
                DisposeForm(generalJournalForm);
                generalJournalForm = null;
                DisposeForm(iarForm);
                iarForm = null;
                DisposeForm(orsBursForm);
                orsBursForm = null;
                DisposeForm(jevForm);
                jevForm = null;
                DisposeForm(devForm);
                devForm = null;
                DisposeForm(transactionLedgerForm);
                transactionLedgerForm = null;

                DisposeForm(userManagementForm);
                userManagementForm = null;

                userManagementForm = new UserManagement();
                userManagementForm.TopLevel = false;
                userManagementForm.FormBorderStyle = FormBorderStyle.None;
                userManagementForm.Dock = DockStyle.Fill;
                userManagementForm.Visible = true;

                panel3.Controls.Add(userManagementForm);
                userManagementForm.Show();
                panel3.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading User Management: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
