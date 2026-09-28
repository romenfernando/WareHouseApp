using System;
using System.Data.SqlClient;
using WareHouseApp.Database;
using WareHouseApp.Exceptions;
using WareHouseApp.People;
using WareHouseApp.Security;

namespace WareHouseApp.Services
{
    /// <summary>
    /// Everything to do with logging in and creating accounts, in one place.
    /// The two forms (Login, Sign Up) call into this instead of talking to the
    /// database or the People classes directly.
    /// </summary>
    public static class AccountService
    {
        /// <summary>
        /// Looks up the username's role, builds the matching Person subtype
        /// (Admin or ShippingOperator) and asks it to verify the password.
        /// This is the polymorphism in action: the caller just gets back a
        /// Person and doesn't need to know or care which subclass it is.
        /// </summary>
        public static Person Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ValidationException("Please enter both a username and a password.");

            string role = LookUpRole(username);

            Person person = role == "Admin" ? (Person)new Admin() : new ShippingOperator();

            if (!person.Login(username, password))
                throw new AuthenticationException("Username or password is incorrect.");

            return person;
        }

        public static void SignUp(string username, string password, string confirmPassword,
                                   string fullName, string email, string role)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ValidationException("Please enter your full name.");
            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("Please choose a username.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ValidationException("Password must be at least 6 characters long.");
            if (password != confirmPassword)
                throw new ValidationException("Passwords do not match.");
            if (role != "Admin" && role != "ShippingOperator")
                throw new ValidationException("Please choose a role.");

            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();

                    using (var checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Employees WHERE Username = @username", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@username", username);
                        int existing = (int)checkCmd.ExecuteScalar();
                        if (existing > 0)
                            throw new DuplicateUserException(
                                "That username is already taken. Please choose another.");
                    }

                    using (var insertCmd = new SqlCommand(
                        "INSERT INTO Employees (Username, PasswordHash, FullName, Email, Role) " +
                        "VALUES (@username, @hash, @fullName, @email, @role)", conn))
                    {
                        insertCmd.Parameters.AddWithValue("@username", username);
                        insertCmd.Parameters.AddWithValue("@hash", PasswordHasher.Hash(password));
                        insertCmd.Parameters.AddWithValue("@fullName", fullName);
                        insertCmd.Parameters.AddWithValue("@email",
                            string.IsNullOrWhiteSpace(email) ? (object)DBNull.Value : email);
                        insertCmd.Parameters.AddWithValue("@role", role);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not create your account.", ex);
            }
        }

        private static string LookUpRole(string username)
        {
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT Role FROM Employees WHERE Username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        object result = cmd.ExecuteScalar();
                        if (result == null)
                            throw new AuthenticationException("Username or password is incorrect.");
                        return result.ToString();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not check that account.", ex);
            }
        }
    }
}
