using System;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;

namespace WindowsFormsApp1.BackendModel
{
    internal class RDBSMConnection
    {
        private static string connectionString = null;

        /// <summary>
        /// Gets or sets the connection string for the database
        /// </summary>
        public static string ConnectionString
        {
            get 
            {
                if (connectionString == null)
                {
                    // Try to get from App.config first
                    try
                    {
                        connectionString = ConfigurationManager.ConnectionStrings["TrackingAPSystem"]?.ConnectionString;
                    }
                    catch
                    {
                        // If not found in config, use default
                    }

                    // If still null, use default connection string
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        connectionString = "Server=localhost;Port=3307;Database=trackingapsystem;Uid=root;Pwd=3rystAl4o8#12;CharSet=utf8;";
                    }
                }
                return connectionString; 
            }
            set { connectionString = value; }
        }

        /// <summary>
        /// Gets a new open database connection
        /// </summary>
        /// <returns>MySqlConnection object (caller is responsible for disposing)</returns>
        public static MySqlConnection GetConnection()
        {
            try
            {
                MySqlConnection connection = new MySqlConnection(ConnectionString);
                connection.Open();
                return connection;
            }
            catch (MySqlException ex)
            {
                string errorMessage = "Unable to connect to MySQL database.\n\n";
                errorMessage += $"Error: {ex.Message}\n\n";
                errorMessage += "Please check:\n";
                errorMessage += "1. MySQL server is running\n";
                errorMessage += "2. Connection settings in App.config are correct\n";
                errorMessage += "3. Database 'trackingapsystem' exists\n";
                errorMessage += "4. User credentials are correct";
                
                throw new Exception(errorMessage, ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Database connection error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if connection is successful, false otherwise</returns>
        public static bool TestConnection()
        {
            try
            {
                using (MySqlConnection testConnection = new MySqlConnection(ConnectionString))
                {
                    testConnection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
