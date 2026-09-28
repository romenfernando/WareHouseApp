using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WareHouseApp.Database;
using WareHouseApp.Exceptions;
using WareHouseApp.Security;

namespace WareHouseApp.Services
{
    /// <summary>
    /// Admin-only employee account management: list, add, edit, reset
    /// password, delete. Shares the same Employees table that Login and
    /// Sign Up use.
    /// </summary>
    public static class EmployeeService
    {
        public static List<WareHouseApp.Employees.EmployeeAccount> GetAll()
        {
            var items = new List<WareHouseApp.Employees.EmployeeAccount>();
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT EmployeeID, Username, FullName, Email, Role FROM Employees ORDER BY FullName", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new WareHouseApp.Employees.EmployeeAccount
                            {
                                EmployeeID = Convert.ToInt32(reader["EmployeeID"]),
                                Username = reader["Username"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"] == DBNull.Value ? "" : reader["Email"].ToString(),
                                Role = reader["Role"].ToString()
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not load the employee list.", ex);
            }
            return items;
        }

        public static void Add(WareHouseApp.Employees.EmployeeAccount employee, string password, string confirmPassword)
        {
            ValidateCommon(employee);
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ValidationException("Password must be at least 6 characters long.");
            if (password != confirmPassword)
                throw new ValidationException("Passwords do not match.");

            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    EnsureUsernameIsFree(conn, employee.Username, null);

                    using (var cmd = new SqlCommand(
                        "INSERT INTO Employees (Username, PasswordHash, FullName, Email, Role, CreatedAt) " +
                        "VALUES (@u, @p, @f, @e, @r, GETDATE())", conn))
                    {
                        cmd.Parameters.AddWithValue("@u", employee.Username);
                        cmd.Parameters.AddWithValue("@p", PasswordHasher.Hash(password));
                        cmd.Parameters.AddWithValue("@f", employee.FullName);
                        cmd.Parameters.AddWithValue("@e",
                            string.IsNullOrWhiteSpace(employee.Email) ? (object)DBNull.Value : employee.Email);
                        cmd.Parameters.AddWithValue("@r", employee.Role);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not add the employee.", ex);
            }
        }

        /// <summary>
        /// Updates the employee's details. If newPassword/confirmPassword are
        /// left blank, the existing password is untouched - filling them in
        /// resets the password as part of the same save.
        /// </summary>
        public static void Update(WareHouseApp.Employees.EmployeeAccount employee, string newPassword, string confirmPassword)
        {
            ValidateCommon(employee);

            bool isResettingPassword =
                !string.IsNullOrWhiteSpace(newPassword) || !string.IsNullOrWhiteSpace(confirmPassword);

            if (isResettingPassword)
            {
                if (newPassword.Length < 6)
                    throw new ValidationException("New password must be at least 6 characters long.");
                if (newPassword != confirmPassword)
                    throw new ValidationException("New password and confirmation do not match.");
            }

            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    EnsureUsernameIsFree(conn, employee.Username, employee.EmployeeID);

                    string sql = isResettingPassword
                        ? "UPDATE Employees SET Username=@u, FullName=@f, Email=@e, Role=@r, PasswordHash=@p WHERE EmployeeID=@id"
                        : "UPDATE Employees SET Username=@u, FullName=@f, Email=@e, Role=@r WHERE EmployeeID=@id";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", employee.Username);
                        cmd.Parameters.AddWithValue("@f", employee.FullName);
                        cmd.Parameters.AddWithValue("@e",
                            string.IsNullOrWhiteSpace(employee.Email) ? (object)DBNull.Value : employee.Email);
                        cmd.Parameters.AddWithValue("@r", employee.Role);
                        cmd.Parameters.AddWithValue("@id", employee.EmployeeID);
                        if (isResettingPassword)
                            cmd.Parameters.AddWithValue("@p", PasswordHasher.Hash(newPassword));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not update the employee.", ex);
            }
        }

        public static void Delete(int employeeId, int currentAdminId)
        {
            if (employeeId == currentAdminId)
                throw new ValidationException("You can't delete the account you're currently logged in as.");

            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("DELETE FROM Employees WHERE EmployeeID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", employeeId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not delete the employee.", ex);
            }
        }

        private static void EnsureUsernameIsFree(SqlConnection conn, string username, int? excludingEmployeeId)
        {
            string sql = excludingEmployeeId == null
                ? "SELECT COUNT(*) FROM Employees WHERE Username=@u"
                : "SELECT COUNT(*) FROM Employees WHERE Username=@u AND EmployeeID <> @id";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@u", username);
                if (excludingEmployeeId != null)
                    cmd.Parameters.AddWithValue("@id", excludingEmployeeId.Value);

                int count = (int)cmd.ExecuteScalar();
                if (count > 0)
                    throw new DuplicateUserException("That username is already taken.");
            }
        }

        private static void ValidateCommon(WareHouseApp.Employees.EmployeeAccount employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Username))
                throw new ValidationException("Please enter a username.");
            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new ValidationException("Please enter the employee's full name.");
            if (employee.Role != "Admin" && employee.Role != "ShippingOperator")
                throw new ValidationException("Please choose a role.");
        }
    }
}
