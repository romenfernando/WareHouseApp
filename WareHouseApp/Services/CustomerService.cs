using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WareHouseApp.Database;
using WareHouseApp.Exceptions;

namespace WareHouseApp.Services
{
    /// <summary>
    /// All database access for customer records - the same four standard
    /// operations as InventoryService, applied to the Customers table.
    /// </summary>
    public static class CustomerService
    {
        public static List<WareHouseApp.Customer.Customer> GetAll()
        {
            var items = new List<WareHouseApp.Customer.Customer>();
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT CustomerID, FullName, Phone, Email, Address, CreatedAt " +
                        "FROM Customers ORDER BY FullName", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new WareHouseApp.Customer.Customer
                            {
                                CustomerID = Convert.ToInt32(reader["CustomerID"]),
                                FullName = reader["FullName"].ToString(),
                                Phone = reader["Phone"] == DBNull.Value ? "" : reader["Phone"].ToString(),
                                Email = reader["Email"] == DBNull.Value ? "" : reader["Email"].ToString(),
                                Address = reader["Address"] == DBNull.Value ? "" : reader["Address"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not load the customer list.", ex);
            }
            return items;
        }

        public static void Add(WareHouseApp.Customer.Customer customer)
        {
            Validate(customer);
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "INSERT INTO Customers (FullName, Phone, Email, Address, CreatedAt) " +
                        "VALUES (@name, @phone, @email, @address, GETDATE())", conn))
                    {
                        AddCommonParams(cmd, customer);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not add the customer.", ex);
            }
        }

        public static void Update(WareHouseApp.Customer.Customer customer)
        {
            Validate(customer);
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE Customers SET FullName=@name, Phone=@phone, Email=@email, Address=@address " +
                        "WHERE CustomerID=@id", conn))
                    {
                        AddCommonParams(cmd, customer);
                        cmd.Parameters.AddWithValue("@id", customer.CustomerID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not update the customer.", ex);
            }
        }

        public static void Delete(int customerId)
        {
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("DELETE FROM Customers WHERE CustomerID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", customerId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not delete the customer.", ex);
            }
        }

        private static void AddCommonParams(SqlCommand cmd, WareHouseApp.Customer.Customer customer)
        {
            cmd.Parameters.AddWithValue("@name", customer.FullName);
            cmd.Parameters.AddWithValue("@phone",
                string.IsNullOrWhiteSpace(customer.Phone) ? (object)DBNull.Value : customer.Phone);
            cmd.Parameters.AddWithValue("@email",
                string.IsNullOrWhiteSpace(customer.Email) ? (object)DBNull.Value : customer.Email);
            cmd.Parameters.AddWithValue("@address",
                string.IsNullOrWhiteSpace(customer.Address) ? (object)DBNull.Value : customer.Address);
        }

        private static void Validate(WareHouseApp.Customer.Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.FullName))
                throw new ValidationException("Please enter the customer's name.");
            if (!string.IsNullOrWhiteSpace(customer.Email) && !customer.Email.Contains("@"))
                throw new ValidationException("Please enter a valid email address.");
        }
    }
}
