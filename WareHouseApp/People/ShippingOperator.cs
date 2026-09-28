using System;
using System.Data.SqlClient;
using WareHouseApp.Database;
using WareHouseApp.Exceptions;
using WareHouseApp.Security;

namespace WareHouseApp.People
{
    /// <summary>
    /// A warehouse/shipping staff member: day-to-day inventory work, no
    /// employee-management access.
    /// </summary>
    public class ShippingOperator : Person
    {
        public override string Role
        {
            get { return "ShippingOperator"; }
        }

        public override bool Login(string username, string password)
        {
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT EmployeeID, PasswordHash, FullName, Email FROM Employees " +
                        "WHERE Username = @username AND Role = @role", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@role", Role);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                                return false;

                            string storedHash = reader["PasswordHash"].ToString();
                            if (!PasswordHasher.Verify(password, storedHash))
                                return false;

                            empID = Convert.ToInt32(reader["EmployeeID"]);
                            userName = username;
                            fullName = reader["FullName"].ToString();
                            email = reader["Email"] == DBNull.Value ? "" : reader["Email"].ToString();
                            return true;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not verify your login details.", ex);
            }
        }

        public override void ChangePassword(string newPassword)
        {
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE Employees SET PasswordHash = @hash WHERE EmployeeID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@hash", PasswordHasher.Hash(newPassword));
                        cmd.Parameters.AddWithValue("@id", empID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not update your password.", ex);
            }
        }

        public override void Logout(int userID)
        {
            empID = 0;
            userName = null;
            fullName = null;
            email = null;
        }

        // Kept from the original starter - will be wired up to the Materials
        // table when the Inventory feature is built.
        public int LoadStocks()
        {
            int stocks = 0;
            return stocks;
        }

        public int ShipStocks()
        {
            int stocks = 0;
            return stocks;
        }
    }
}
