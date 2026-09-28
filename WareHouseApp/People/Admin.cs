using System;
using System.Data.SqlClient;
using WareHouseApp.Database;
using WareHouseApp.Exceptions;
using WareHouseApp.Security;

namespace WareHouseApp.People
{
    /// <summary>
    /// An administrator: full access, including employee management.
    /// </summary>
    public class Admin : Person
    {
        public override string Role
        {
            get { return "Admin"; }
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
                                return false; // no Admin account with that username

                            string storedHash = reader["PasswordHash"].ToString();
                            if (!PasswordHasher.Verify(password, storedHash))
                                return false; // wrong password

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
    }
}
