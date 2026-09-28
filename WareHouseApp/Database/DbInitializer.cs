using System.Data.SqlClient;
using WareHouseApp.Exceptions;
using WareHouseApp.Security;

namespace WareHouseApp.Database
{
    /// <summary>
    /// Creates the database tables the first time the app is run against a fresh
    /// WarehouseDB.mdf, so a marker/tutor can just open the project and run it -
    /// no manual SQL scripts to execute by hand. Also seeds one default Admin
    /// account so there is always a way to log in.
    /// </summary>
    public static class DbInitializer
    {
        public static void EnsureCreated()
        {
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    Execute(conn, @"
IF OBJECT_ID('dbo.Employees', 'U') IS NULL
CREATE TABLE Employees (
    EmployeeID   INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(100) NULL,
    Role         NVARCHAR(20)  NOT NULL,
    CreatedAt    DATETIME      NOT NULL DEFAULT GETDATE()
);");

                    Execute(conn, @"
IF OBJECT_ID('dbo.Customers', 'U') IS NULL
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    FullName   NVARCHAR(100) NOT NULL,
    Phone      NVARCHAR(20)  NULL,
    Email      NVARCHAR(100) NULL,
    Address    NVARCHAR(200) NULL,
    CreatedAt  DATETIME      NOT NULL DEFAULT GETDATE()
);");

                    Execute(conn, @"
IF OBJECT_ID('dbo.Materials', 'U') IS NULL
CREATE TABLE Materials (
    MaterialID    INT IDENTITY(1,1) PRIMARY KEY,
    MaterialName  NVARCHAR(100)  NOT NULL,
    Category      NVARCHAR(50)   NULL,
    Quantity      INT            NOT NULL DEFAULT 0,
    UnitPrice     DECIMAL(10,2)  NOT NULL DEFAULT 0,
    ReorderLevel  INT            NOT NULL DEFAULT 0,
    LastUpdated   DATETIME       NOT NULL DEFAULT GETDATE()
);");

                    SeedDefaultAdmin(conn);
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException(
                    "Could not set up the database. Check that SQL Server LocalDB is installed.", ex);
            }
        }

        private static void SeedDefaultAdmin(SqlConnection conn)
        {
            using (var checkCmd = new SqlCommand("SELECT COUNT(*) FROM Employees", conn))
            {
                int existingCount = (int)checkCmd.ExecuteScalar();
                if (existingCount > 0) return;

                string hash = PasswordHasher.Hash("admin123");
                using (var seedCmd = new SqlCommand(
                    "INSERT INTO Employees (Username, PasswordHash, FullName, Email, Role) " +
                    "VALUES ('Admin', @hash, 'Default Administrator', 'admin@warehouse.local', 'Admin')", conn))
                {
                    seedCmd.Parameters.AddWithValue("@hash", hash);
                    seedCmd.ExecuteNonQuery();
                }
            }
        }

        private static void Execute(SqlConnection conn, string sql)
        {
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
