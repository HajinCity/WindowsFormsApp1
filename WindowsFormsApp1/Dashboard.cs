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
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApp1
{
    public partial class Dashboard : Form
    {
        private DateTime? filterStartDate = null;
        private DateTime? filterEndDate = null;
        private string selectedMonthFilter = null; // Format: "YYYY-MM" or null for all months

        public Dashboard()
        {
            InitializeComponent();
            InitializeCharts();
            InitializeMonthSelector();
            WireUpEvents();
            LoadDashboardData();
        }

        private void InitializeMonthSelector()
        {
            // Look for a ComboBox control for month selection
            // If you have a ComboBox in the designer, replace "monthComboBox" with its actual name
            var monthComboBox = this.Controls.Find("monthComboBox", true).FirstOrDefault() as ComboBox;
            
            if (monthComboBox == null)
            {
                // Try to find any ComboBox in gradientPanel1 or panel5
                monthComboBox = gradientPanel1?.Controls.OfType<ComboBox>().FirstOrDefault();
            }

            if (monthComboBox != null)
            {
                monthComboBox.Items.Clear();
                monthComboBox.Items.Add("All Months");
                monthComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                
                // Populate with available months from database (last 24 months)
                PopulateMonthSelector(monthComboBox);
                
                monthComboBox.SelectedIndex = 0; // Default to "All Months"
                monthComboBox.SelectedIndexChanged += MonthComboBox_SelectedIndexChanged;
            }
        }

        private void PopulateMonthSelector(ComboBox comboBox)
        {
            try
            {
                var monthSet = new HashSet<string>();
                
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    // Get distinct months from both ora_burono and sbl_po
                    string query = @"SELECT DISTINCT DATE_FORMAT(date, '%Y-%m') AS month_key,
                                           DATE_FORMAT(date, '%B %Y') AS month_display
                                    FROM ora_burono
                                    WHERE date >= DATE_SUB(CURDATE(), INTERVAL 24 MONTH)
                                    UNION
                                    SELECT DISTINCT DATE_FORMAT(date_paid, '%Y-%m') AS month_key,
                                           DATE_FORMAT(date_paid, '%B %Y') AS month_display
                                    FROM sbl_po
                                    WHERE date_paid >= DATE_SUB(CURDATE(), INTERVAL 24 MONTH)
                                    ORDER BY month_key DESC";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string monthKey = reader["month_key"].ToString();
                            if (!monthSet.Contains(monthKey))
                            {
                                monthSet.Add(monthKey);
                                string monthDisplay = reader["month_display"].ToString();
                                comboBox.Items.Add(new MonthItem { Key = monthKey, Display = monthDisplay });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // If error, just add recent months manually
                for (int i = 0; i < 24; i++)
                {
                    DateTime month = DateTime.Today.AddMonths(-i);
                    string monthKey = month.ToString("yyyy-MM");
                    string monthDisplay = month.ToString("MMMM yyyy");
                    comboBox.Items.Add(new MonthItem { Key = monthKey, Display = monthDisplay });
                }
            }
        }

        private class MonthItem
        {
            public string Key { get; set; }
            public string Display { get; set; }
            public override string ToString() => Display;
        }

        private void MonthComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;

            if (comboBox.SelectedIndex == 0 || comboBox.SelectedItem?.ToString() == "All Months")
            {
                selectedMonthFilter = null;
            }
            else
            {
                // Extract month key from selected item
                var selectedItem = comboBox.SelectedItem as MonthItem;
                if (selectedItem != null)
                {
                    selectedMonthFilter = selectedItem.Key;
                }
            }

            LoadDashboardData();
        }

        private void WireUpEvents()
        {
            if (genCustombtn != null)
            {
                genCustombtn.Click += GenCustombtn_Click;
            }
            if (exportPHCsv != null)
            {
                exportPHCsv.Click += ExportPHCsv_Click;
            }
            if (exportcsv != null)
            {
                exportcsv.Click += Exportcsv_Click;
            }
            this.Load += Dashboard_Load;
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            LoadDashboardData();
        }

        private void InitializeCharts()
        {
            // Initialize chart1 - Column Chart with 3 series
            if (chart1 != null)
            {
                // Ensure we have 3 series
                while (chart1.Series.Count < 3)
                {
                    chart1.Series.Add(new Series());
                }

                chart1.Series[0].Name = "Paid";
                chart1.Series[0].ChartType = SeriesChartType.Column;
                chart1.Series[0].Color = Color.Green;
                chart1.Series[0].ChartArea = "ChartArea1";
                chart1.Series[0].Legend = "Legend1";
                chart1.Series[0].IsValueShownAsLabel = false;
                chart1.Series[0].ToolTip = "#VALX\nPaid: ₱#VALY{N2}";

                chart1.Series[1].Name = "Unpaid";
                chart1.Series[1].ChartType = SeriesChartType.Column;
                chart1.Series[1].Color = Color.Red;
                chart1.Series[1].ChartArea = "ChartArea1";
                chart1.Series[1].Legend = "Legend1";
                chart1.Series[1].IsValueShownAsLabel = false;
                chart1.Series[1].ToolTip = "#VALX\nUnpaid: ₱#VALY{N2}";

                chart1.Series[2].Name = "Partially Paid";
                chart1.Series[2].ChartType = SeriesChartType.Column;
                chart1.Series[2].Color = Color.Yellow;
                chart1.Series[2].ChartArea = "ChartArea1";
                chart1.Series[2].Legend = "Legend1";
                chart1.Series[2].IsValueShownAsLabel = false;
                chart1.Series[2].ToolTip = "#VALX\nPartially Paid: ₱#VALY{N2}";

                // Enable tooltips
                chart1.IsSoftShadows = false;
            }

            // Initialize chart2 - Pie Chart
            if (chart2 != null && chart2.Series.Count > 0)
            {
                chart2.Series[0].ChartType = SeriesChartType.Pie;
                chart2.Series[0].IsValueShownAsLabel = true;
                chart2.Series[0].LabelFormat = "{0}%";
            }

            // Initialize SplineArea charts
            if (paid != null && paid.Series.Count > 0)
            {
                paid.Series[0].ChartType = SeriesChartType.SplineArea;
                paid.Series[0].Color = Color.Green;
                paid.Series[0].Name = "Paid";
                paid.Series[0].ToolTip = "#VALX\nPaid: ₱#VALY{N2}";
            }

            if (unpaid != null && unpaid.Series.Count > 0)
            {
                unpaid.Series[0].ChartType = SeriesChartType.SplineArea;
                unpaid.Series[0].Color = Color.Red;
                unpaid.Series[0].Name = "Unpaid";
                unpaid.Series[0].ToolTip = "#VALX\nUnpaid: ₱#VALY{N2}";
            }

            if (partiallypaid != null && partiallypaid.Series.Count > 0)
            {
                partiallypaid.Series[0].ChartType = SeriesChartType.SplineArea;
                partiallypaid.Series[0].Color = Color.Yellow;
                partiallypaid.Series[0].Name = "Partially Paid";
                partiallypaid.Series[0].ToolTip = "#VALX\nPartially Paid: ₱#VALY{N2}";
            }
        }

        private void GenCustombtn_Click(object sender, EventArgs e)
        {
            if (dateTimePicker2 != null && dateTimePicker3 != null)
            {
                if (dateTimePicker2.Value.Date > dateTimePicker3.Value.Date)
                {
                    MessageBox.Show(
                        "Start date cannot be greater than end date. Please adjust the date range.",
                        "Invalid Date Range",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                filterStartDate = dateTimePicker2.Value.Date;
                filterEndDate = dateTimePicker3.Value.Date;
                LoadDashboardData();
            }
        }

        private void LoadDashboardData()
        {
            try
            {
                var totals = CalculateTotals();
                UpdateLabels(totals);
                var monthlyData = GetMonthlyData();
                UpdateCharts(totals, monthlyData);
                LoadPaymentHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading dashboard data: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private (decimal totalPayables, decimal totalPaid, decimal totalUnpaid, decimal totalPartiallyPaid) CalculateTotals()
        {
            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    // Build query with optional date range filter and month filter
                    string dateFilter = "";
                    string monthFilter = "";
                    
                    if (!string.IsNullOrEmpty(selectedMonthFilter))
                    {
                        // Filter by specific month
                        monthFilter = @"AND DATE_FORMAT(date, '%Y-%m') = @selectedMonth";
                    }
                    else if (filterStartDate.HasValue && filterEndDate.HasValue)
                    {
                        dateFilter = @"AND date BETWEEN @startDate AND @endDate";
                    }

                    // Total Payables
                    string queryPayables = $@"SELECT SUM(COALESCE(payable_amount, 0)) AS total 
                                             FROM ora_burono 
                                             WHERE 1=1 {dateFilter} {monthFilter}";
                    decimal totalPayables = 0;
                    using (MySqlCommand cmd = new MySqlCommand(queryPayables, connection))
                    {
                        if (!string.IsNullOrEmpty(selectedMonthFilter))
                        {
                            cmd.Parameters.AddWithValue("@selectedMonth", selectedMonthFilter);
                        }
                        else if (filterStartDate.HasValue && filterEndDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@startDate", filterStartDate.Value);
                            cmd.Parameters.AddWithValue("@endDate", filterEndDate.Value);
                        }
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalPayables = Convert.ToDecimal(result);
                        }
                    }

                    // Total Paid (from ora_burono: payable_amount - balance)
                    string queryPaid = $@"SELECT SUM(COALESCE(payable_amount, 0) - COALESCE(balance, 0)) AS total 
                                         FROM ora_burono 
                                         WHERE 1=1 {dateFilter} {monthFilter}";
                    decimal totalPaid = 0;
                    using (MySqlCommand cmd = new MySqlCommand(queryPaid, connection))
                    {
                        if (!string.IsNullOrEmpty(selectedMonthFilter))
                        {
                            cmd.Parameters.AddWithValue("@selectedMonth", selectedMonthFilter);
                        }
                        else if (filterStartDate.HasValue && filterEndDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@startDate", filterStartDate.Value);
                            cmd.Parameters.AddWithValue("@endDate", filterEndDate.Value);
                        }
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalPaid = Convert.ToDecimal(result);
                        }
                    }

                    // Total Unpaid - Sum of all balances in ora_burono
                    string queryUnpaid = $@"SELECT SUM(COALESCE(balance, 0)) AS total 
                                           FROM ora_burono 
                                           WHERE 1=1 {dateFilter} {monthFilter}";
                    decimal totalUnpaid = 0;
                    using (MySqlCommand cmd = new MySqlCommand(queryUnpaid, connection))
                    {
                        if (!string.IsNullOrEmpty(selectedMonthFilter))
                        {
                            cmd.Parameters.AddWithValue("@selectedMonth", selectedMonthFilter);
                        }
                        else if (filterStartDate.HasValue && filterEndDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@startDate", filterStartDate.Value);
                            cmd.Parameters.AddWithValue("@endDate", filterEndDate.Value);
                        }
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalUnpaid = Convert.ToDecimal(result);
                        }
                    }

                    // Total Partially Paid - Sum of all amount_paid from sbl_po
                    string dateFilterSblPo = "";
                    string monthFilterSblPo = "";
                    if (!string.IsNullOrEmpty(selectedMonthFilter))
                    {
                        monthFilterSblPo = "WHERE DATE_FORMAT(date_paid, '%Y-%m') = @selectedMonth";
                    }
                    else if (filterStartDate.HasValue && filterEndDate.HasValue)
                    {
                        dateFilterSblPo = "WHERE date_paid BETWEEN @startDate AND @endDate";
                    }
                    string queryPartiallyPaid = $@"SELECT SUM(COALESCE(amount_paid, 0)) AS total 
                                                    FROM sbl_po 
                                                    {dateFilterSblPo} {monthFilterSblPo}";
                    decimal totalPartiallyPaid = 0;
                    using (MySqlCommand cmd = new MySqlCommand(queryPartiallyPaid, connection))
                    {
                        if (!string.IsNullOrEmpty(selectedMonthFilter))
                        {
                            cmd.Parameters.AddWithValue("@selectedMonth", selectedMonthFilter);
                        }
                        else if (filterStartDate.HasValue && filterEndDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@startDate", filterStartDate.Value);
                            cmd.Parameters.AddWithValue("@endDate", filterEndDate.Value);
                        }
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            totalPartiallyPaid = Convert.ToDecimal(result);
                        }
                    }

                    return (totalPayables, totalPaid, totalUnpaid, totalPartiallyPaid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating totals: {ex.Message}", "Calculation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return (0, 0, 0, 0);
            }
        }

        private void UpdateLabels((decimal totalPayables, decimal totalPaid, decimal totalUnpaid, decimal totalPartiallyPaid) totals)
        {
            if (totalpaybles != null)
            {
                totalpaybles.Text = FormatAmount(totals.totalPayables);
            }
            if (totalpaid != null)
            {
                totalpaid.Text = FormatAmount(totals.totalPaid);
            }
            if (totalunpaid != null)
            {
                totalunpaid.Text = FormatAmount(totals.totalUnpaid);
            }
            if (totalpartiallypaid != null)
            {
                totalpartiallypaid.Text = FormatAmount(totals.totalPartiallyPaid);
            }
        }

        private string FormatAmount(decimal amount)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:N2}", amount);
        }

        private Dictionary<string, (decimal paid, decimal unpaid, decimal partiallyPaid)> GetMonthlyData()
        {
            var monthlyData = new Dictionary<string, (decimal paid, decimal unpaid, decimal partiallyPaid)>();

            try
            {
                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    // Determine date range - default to last 12 months if no filter
                    DateTime startDate = filterStartDate ?? DateTime.Today.AddMonths(-12);
                    DateTime endDate = filterEndDate ?? DateTime.Today;

                    // First, get all months in the range to ensure we have complete data
                    var allMonths = new List<string>();
                    DateTime currentMonth = new DateTime(startDate.Year, startDate.Month, 1);
                    while (currentMonth <= endDate)
                    {
                        string monthKey = currentMonth.ToString("yyyy-MM");
                        allMonths.Add(monthKey);
                        if (!monthlyData.ContainsKey(monthKey))
                        {
                            monthlyData[monthKey] = (0, 0, 0);
                        }
                        currentMonth = currentMonth.AddMonths(1);
                    }

                    // Get monthly data for Paid amounts (from sbl_po - amount_paid)
                    string queryPaid = @"SELECT DATE_FORMAT(date_paid, '%Y-%m') AS month_key,
                                                DATE_FORMAT(date_paid, '%b') AS month_name,
                                                SUM(COALESCE(amount_paid, 0)) AS total_paid
                                         FROM sbl_po
                                         WHERE date_paid BETWEEN @startDate AND @endDate
                                         GROUP BY DATE_FORMAT(date_paid, '%Y-%m'), DATE_FORMAT(date_paid, '%b')
                                         ORDER BY month_key";

                    using (MySqlCommand cmd = new MySqlCommand(queryPaid, connection))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string monthKey = reader["month_key"].ToString();
                                string monthName = reader["month_name"].ToString();
                                decimal paid = reader["total_paid"] == DBNull.Value ? 0 : reader.GetDecimal("total_paid");

                                if (!monthlyData.ContainsKey(monthKey))
                                {
                                    monthlyData[monthKey] = (0, 0, 0);
                                }
                                var current = monthlyData[monthKey];
                                monthlyData[monthKey] = (paid, current.unpaid, current.partiallyPaid);
                            }
                        }
                    }

                    // Get monthly data for Unpaid amounts - calculate balance at end of each month
                    // This shows the cumulative unpaid balance for records created up to that month
                    foreach (string monthKey in allMonths)
                    {
                        string[] parts = monthKey.Split('-');
                        int year = int.Parse(parts[0]);
                        int month = int.Parse(parts[1]);
                        DateTime monthEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                        string queryUnpaid = @"SELECT SUM(COALESCE(balance, 0)) AS total_unpaid
                                               FROM ora_burono
                                               WHERE date <= @monthEnd";

                        using (MySqlCommand cmd = new MySqlCommand(queryUnpaid, connection))
                        {
                            cmd.Parameters.AddWithValue("@monthEnd", monthEnd);

                            object result = cmd.ExecuteScalar();
                            decimal unpaid = 0;
                            if (result != null && result != DBNull.Value)
                            {
                                unpaid = Convert.ToDecimal(result);
                            }

                            var current = monthlyData[monthKey];
                            monthlyData[monthKey] = (current.paid, unpaid, current.partiallyPaid);
                        }
                    }

                    // Get monthly data for Partially Paid amounts (sum of all amount_paid from sbl_po)
                    string queryPartiallyPaid = @"SELECT DATE_FORMAT(date_paid, '%Y-%m') AS month_key,
                                                         DATE_FORMAT(date_paid, '%b') AS month_name,
                                                         SUM(COALESCE(amount_paid, 0)) AS total_partially_paid
                                                  FROM sbl_po
                                                  WHERE date_paid BETWEEN @startDate AND @endDate
                                                  GROUP BY DATE_FORMAT(date_paid, '%Y-%m'), DATE_FORMAT(date_paid, '%b')
                                                  ORDER BY month_key";

                    using (MySqlCommand cmd = new MySqlCommand(queryPartiallyPaid, connection))
                    {
                        cmd.Parameters.AddWithValue("@startDate", startDate);
                        cmd.Parameters.AddWithValue("@endDate", endDate);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string monthKey = reader["month_key"].ToString();
                                string monthName = reader["month_name"].ToString();
                                decimal partiallyPaid = reader["total_partially_paid"] == DBNull.Value ? 0 : reader.GetDecimal("total_partially_paid");

                                if (!monthlyData.ContainsKey(monthKey))
                                {
                                    monthlyData[monthKey] = (0, 0, 0);
                                }
                                var current = monthlyData[monthKey];
                                monthlyData[monthKey] = (current.paid, current.unpaid, partiallyPaid);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting monthly data: {ex.Message}", "Data Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return monthlyData;
        }

        private void UpdateCharts((decimal totalPayables, decimal totalPaid, decimal totalUnpaid, decimal totalPartiallyPaid) totals,
                                   Dictionary<string, (decimal paid, decimal unpaid, decimal partiallyPaid)> monthlyData)
        {
            // Update chart1 - Column Chart with monthly data
            if (chart1 != null && chart1.Series.Count >= 3)
            {
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                chart1.Series[2].Points.Clear();

                if (monthlyData.Count > 0)
                {
                    // Sort months chronologically
                    var sortedMonths = monthlyData.OrderBy(kvp => kvp.Key).ToList();

                    foreach (var month in sortedMonths)
                    {
                        // Extract month name from key (YYYY-MM format)
                        string[] parts = month.Key.Split('-');
                        string monthName = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1).ToString("MMM");
                        string fullMonthName = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1).ToString("MMMM yyyy");

                        // Create tooltip text with all three values
                        string tooltipText = $"{fullMonthName}\nPaid: ₱{month.Value.paid:N2}\nUnpaid: ₱{month.Value.unpaid:N2}\nPartially Paid: ₱{month.Value.partiallyPaid:N2}";

                        // Add points and set tooltips
                        int paidIndex = chart1.Series[0].Points.AddXY(monthName, (double)month.Value.paid);
                        chart1.Series[0].Points[paidIndex].ToolTip = tooltipText;

                        int unpaidIndex = chart1.Series[1].Points.AddXY(monthName, (double)month.Value.unpaid);
                        chart1.Series[1].Points[unpaidIndex].ToolTip = tooltipText;

                        int partiallyPaidIndex = chart1.Series[2].Points.AddXY(monthName, (double)month.Value.partiallyPaid);
                        chart1.Series[2].Points[partiallyPaidIndex].ToolTip = tooltipText;
                    }
                }
            }

            // Update chart2 - Pie Chart with percentages (still using totals)
            if (chart2 != null && chart2.Series.Count > 0)
            {
                chart2.Series[0].Points.Clear();
                decimal total = totals.totalPaid + totals.totalUnpaid + totals.totalPartiallyPaid;
                
                if (total > 0)
                {
                    double paidPercent = (double)(totals.totalPaid / total * 100);
                    double unpaidPercent = (double)(totals.totalUnpaid / total * 100);
                    double partiallyPaidPercent = (double)(totals.totalPartiallyPaid / total * 100);

                    chart2.Series[0].Points.AddXY("Paid", paidPercent);
                    chart2.Series[0].Points[0].Color = Color.Green;
                    
                    chart2.Series[0].Points.AddXY("Unpaid", unpaidPercent);
                    chart2.Series[0].Points[1].Color = Color.Red;
                    
                    chart2.Series[0].Points.AddXY("Partially Paid", partiallyPaidPercent);
                    chart2.Series[0].Points[2].Color = Color.Yellow;
                }
            }

            // Update SplineArea charts with monthly data
            if (paid != null && paid.Series.Count > 0)
            {
                paid.Series[0].Points.Clear();
                if (monthlyData.Count > 0)
                {
                    var sortedMonths = monthlyData.OrderBy(kvp => kvp.Key).ToList();
                    foreach (var month in sortedMonths)
                    {
                        string[] parts = month.Key.Split('-');
                        string monthName = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1).ToString("MMM");
                        paid.Series[0].Points.AddXY(monthName, (double)month.Value.paid);
                    }
                }
            }

            if (unpaid != null && unpaid.Series.Count > 0)
            {
                unpaid.Series[0].Points.Clear();
                if (monthlyData.Count > 0)
                {
                    var sortedMonths = monthlyData.OrderBy(kvp => kvp.Key).ToList();
                    foreach (var month in sortedMonths)
                    {
                        string[] parts = month.Key.Split('-');
                        string monthName = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1).ToString("MMM");
                        unpaid.Series[0].Points.AddXY(monthName, (double)month.Value.unpaid);
                    }
                }
            }

            if (partiallypaid != null && partiallypaid.Series.Count > 0)
            {
                partiallypaid.Series[0].Points.Clear();
                if (monthlyData.Count > 0)
                {
                    var sortedMonths = monthlyData.OrderBy(kvp => kvp.Key).ToList();
                    foreach (var month in sortedMonths)
                    {
                        string[] parts = month.Key.Split('-');
                        string monthName = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1).ToString("MMM");
                        partiallypaid.Series[0].Points.AddXY(monthName, (double)month.Value.partiallyPaid);
                    }
                }
            }
        }

        private void LoadPaymentHistory()
        {
            try
            {
                if (PaymentHistory == null) return;

                PaymentHistory.Rows.Clear();

                using (MySqlConnection connection = RDBSMConnection.GetConnection())
                {
                    string dateFilter = "";
                    string monthFilter = "";
                    
                    if (!string.IsNullOrEmpty(selectedMonthFilter))
                    {
                        monthFilter = "WHERE DATE_FORMAT(date_paid, '%Y-%m') = @selectedMonth";
                    }
                    else if (filterStartDate.HasValue && filterEndDate.HasValue)
                    {
                        dateFilter = "WHERE date_paid BETWEEN @startDate AND @endDate";
                    }

                    string query = $@"SELECT date_paid, supplier, po_amount, amount_paid, balance
                                     FROM sbl_po
                                     {dateFilter} {monthFilter}
                                     ORDER BY date_paid DESC, sbl_id DESC";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(selectedMonthFilter))
                        {
                            command.Parameters.AddWithValue("@selectedMonth", selectedMonthFilter);
                        }
                        else if (filterStartDate.HasValue && filterEndDate.HasValue)
                        {
                            command.Parameters.AddWithValue("@startDate", filterStartDate.Value);
                            command.Parameters.AddWithValue("@endDate", filterEndDate.Value);
                        }

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string datePaid = reader["date_paid"] == DBNull.Value 
                                    ? "" 
                                    : reader.GetDateTime("date_paid").ToShortDateString();
                                string supplier = reader["supplier"]?.ToString() ?? "";
                                string poAmount = FormatAmountDisplay(reader["po_amount"]?.ToString() ?? "");
                                string amountPaid = FormatAmountDisplay(reader["amount_paid"]?.ToString() ?? "");
                                string balance = FormatAmountDisplay(reader["balance"]?.ToString() ?? "");

                                PaymentHistory.Rows.Add(datePaid, supplier, poAmount, amountPaid, balance);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unable to load payment history: {ex.Message}",
                    "Load Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string FormatAmountDisplay(string amountValue)
        {
            if (string.IsNullOrWhiteSpace(amountValue))
            {
                return amountValue ?? string.Empty;
            }

            string clean = amountValue.Replace(",", "").Trim();
            if (decimal.TryParse(clean, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out decimal numericValue))
            {
                string formattedInteger = string.Format(CultureInfo.InvariantCulture, "{0:N0}", Math.Truncate(numericValue));
                int decimalIndex = clean.IndexOf('.');
                string fractionalPart = decimalIndex >= 0 ? clean.Substring(decimalIndex) : string.Empty;
                return formattedInteger + fractionalPart;
            }

            return amountValue;
        }

        private void ExportPHCsv_Click(object sender, EventArgs e)
        {
            if (PaymentHistory == null || PaymentHistory.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export Payment History to CSV";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.FileName = $"PaymentHistory_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write header row
                        writer.WriteLine("Date Paid,Supplier,Payable Amount,Amount Paid,Balance");

                        // Write data rows
                        foreach (DataGridViewRow row in PaymentHistory.Rows)
                        {
                            if (row.IsNewRow)
                            {
                                continue;
                            }

                            string datePaid = EscapeForCsv(row.Cells[0].Value?.ToString());
                            string supplier = EscapeForCsv(row.Cells[1].Value?.ToString());
                            string poAmount = EscapeForCsv(row.Cells[2].Value?.ToString());
                            string amountPaid = EscapeForCsv(row.Cells[3].Value?.ToString());
                            string balance = EscapeForCsv(row.Cells[4].Value?.ToString());

                            writer.WriteLine($"{datePaid},{supplier},{poAmount},{amountPaid},{balance}");
                        }
                    }

                    MessageBox.Show("Payment history exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export payment history: {ex.Message}", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Exportcsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export Dashboard Data to CSV";
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                string dateRange = "";
                if (filterStartDate.HasValue && filterEndDate.HasValue)
                {
                    dateRange = $"{filterStartDate.Value:yyyyMMdd}_{filterEndDate.Value:yyyyMMdd}";
                }
                else
                {
                    dateRange = DateTime.Now.ToString("yyyyMMdd");
                }
                saveFileDialog.FileName = $"Dashboard_Export_{dateRange}_{DateTime.Now:HHmmss}.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write header
                        writer.WriteLine("DASHBOARD EXPORT");
                        writer.WriteLine("================");
                        writer.WriteLine($"Export Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        if (filterStartDate.HasValue && filterEndDate.HasValue)
                        {
                            writer.WriteLine($"Date Range: {filterStartDate.Value:yyyy-MM-dd} to {filterEndDate.Value:yyyy-MM-dd}");
                        }
                        else
                        {
                            writer.WriteLine("Date Range: All Data");
                        }
                        writer.WriteLine();

                        // Write Summary Totals Section
                        writer.WriteLine("SUMMARY TOTALS");
                        writer.WriteLine("==============");
                        writer.WriteLine("Category,Amount");
                        writer.WriteLine($"Total Payables,{EscapeForCsv(totalpaybles?.Text ?? "0.00")}");
                        writer.WriteLine($"Total Paid,{EscapeForCsv(totalpaid?.Text ?? "0.00")}");
                        writer.WriteLine($"Total Unpaid,{EscapeForCsv(totalunpaid?.Text ?? "0.00")}");
                        writer.WriteLine($"Total Partially Paid,{EscapeForCsv(totalpartiallypaid?.Text ?? "0.00")}");
                        writer.WriteLine();
                        writer.WriteLine();

                        // Write Monthly Chart Data Section
                        var monthlyData = GetMonthlyData();
                        if (monthlyData.Count > 0)
                        {
                            writer.WriteLine("MONTHLY PAYMENT TRENDS");
                            writer.WriteLine("======================");
                            writer.WriteLine("Month,Paid Amount,Unpaid Amount,Partially Paid Amount");

                            var sortedMonths = monthlyData.OrderBy(kvp => kvp.Key).ToList();
                            foreach (var month in sortedMonths)
                            {
                                string[] parts = month.Key.Split('-');
                                // Format month as "October 2025" to avoid splitting in Excel
                                string monthName = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1).ToString("MMMM yyyy");
                                // Format amounts with proper currency formatting (with thousands separator)
                                string paidAmount = month.Value.paid.ToString("#,##0.00", CultureInfo.InvariantCulture);
                                string unpaidAmount = month.Value.unpaid.ToString("#,##0.00", CultureInfo.InvariantCulture);
                                string partiallyPaidAmount = month.Value.partiallyPaid.ToString("#,##0.00", CultureInfo.InvariantCulture);
                                
                                writer.WriteLine($"{EscapeForCsv(monthName)},{paidAmount},{unpaidAmount},{partiallyPaidAmount}");
                            }
                            writer.WriteLine();
                            writer.WriteLine();
                        }

                        // Write Payment History Section
                        if (PaymentHistory != null && PaymentHistory.Rows.Count > 0)
                        {
                            writer.WriteLine("PAYMENT HISTORY");
                            writer.WriteLine("===============");
                            writer.WriteLine("Date Paid,Supplier,Payable Amount,Amount Paid,Balance");

                            foreach (DataGridViewRow row in PaymentHistory.Rows)
                            {
                                if (row.IsNewRow)
                                {
                                    continue;
                                }

                                string datePaid = EscapeForCsv(row.Cells[0].Value?.ToString());
                                string supplier = EscapeForCsv(row.Cells[1].Value?.ToString());
                                string poAmount = EscapeForCsv(row.Cells[2].Value?.ToString());
                                string amountPaid = EscapeForCsv(row.Cells[3].Value?.ToString());
                                string balance = EscapeForCsv(row.Cells[4].Value?.ToString());

                                writer.WriteLine($"{datePaid},{supplier},{poAmount},{amountPaid},{balance}");
                            }
                        }
                    }

                    MessageBox.Show("Dashboard data exported successfully.", "Export Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to export dashboard data: {ex.Message}", "Export Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string EscapeForCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n");
            string escaped = value.Replace("\"", "\"\"");
            return mustQuote ? $"\"{escaped}\"" : escaped;
        }
    }
}
