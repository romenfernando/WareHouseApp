using System.Configuration;
using System.Data.SqlClient;

namespace WareHouseApp.Database
{
    /// <summary>
    /// Single place that knows how to open a connection to the warehouse database.
    /// Every feature (login, inventory, customers, employees) goes through here
    /// instead of building its own connection string, so there is one place to
    /// change if the database ever moves.
    /// </summary>
    public static class DbHelper
    {
        public static SqlConnection GetConnection()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["WarehouseDbConnection"].ConnectionString;
            return new SqlConnection(connectionString);
        }
    }
}
