using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using WareHouseApp.Database;
using WareHouseApp.Exceptions;

namespace WareHouseApp.Services
{
    /// <summary>
    /// All database access for inventory items, in one place, with the
    /// standard four operations (Create, Read, Update, Delete).
    /// </summary>
    public static class InventoryService
    {
        public static List<WareHouseApp.Material.Material> GetAll()
        {
            var items = new List<WareHouseApp.Material.Material>();
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "SELECT MaterialID, MaterialName, Category, Quantity, UnitPrice, ReorderLevel, LastUpdated " +
                        "FROM Materials ORDER BY MaterialName", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new WareHouseApp.Material.Material
                            {
                                MaterialID = Convert.ToInt32(reader["MaterialID"]),
                                MaterialName = reader["MaterialName"].ToString(),
                                Category = reader["Category"] == DBNull.Value ? "" : reader["Category"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                UnitPrice = Convert.ToDecimal(reader["UnitPrice"]),
                                ReorderLevel = Convert.ToInt32(reader["ReorderLevel"]),
                                LastUpdated = Convert.ToDateTime(reader["LastUpdated"])
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not load the inventory list.", ex);
            }
            return items;
        }

        public static void Add(WareHouseApp.Material.Material item)
        {
            Validate(item);
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "INSERT INTO Materials (MaterialName, Category, Quantity, UnitPrice, ReorderLevel, LastUpdated) " +
                        "VALUES (@name, @cat, @qty, @price, @reorder, GETDATE())", conn))
                    {
                        AddCommonParams(cmd, item);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not add the item.", ex);
            }
        }

        public static void Update(WareHouseApp.Material.Material item)
        {
            Validate(item);
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "UPDATE Materials SET MaterialName=@name, Category=@cat, Quantity=@qty, " +
                        "UnitPrice=@price, ReorderLevel=@reorder, LastUpdated=GETDATE() WHERE MaterialID=@id", conn))
                    {
                        AddCommonParams(cmd, item);
                        cmd.Parameters.AddWithValue("@id", item.MaterialID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not update the item.", ex);
            }
        }

        public static void Delete(int materialId)
        {
            try
            {
                using (SqlConnection conn = DbHelper.GetConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("DELETE FROM Materials WHERE MaterialID=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", materialId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DatabaseOperationException("Could not delete the item.", ex);
            }
        }

        private static void AddCommonParams(SqlCommand cmd, WareHouseApp.Material.Material item)
        {
            cmd.Parameters.AddWithValue("@name", item.MaterialName);
            cmd.Parameters.AddWithValue("@cat",
                string.IsNullOrWhiteSpace(item.Category) ? (object)DBNull.Value : item.Category);
            cmd.Parameters.AddWithValue("@qty", item.Quantity);
            cmd.Parameters.AddWithValue("@price", item.UnitPrice);
            cmd.Parameters.AddWithValue("@reorder", item.ReorderLevel);
        }

        private static void Validate(WareHouseApp.Material.Material item)
        {
            if (string.IsNullOrWhiteSpace(item.MaterialName))
                throw new ValidationException("Please enter an item name.");
            if (item.Quantity < 0)
                throw new ValidationException("Quantity cannot be negative.");
            if (item.UnitPrice < 0)
                throw new ValidationException("Unit price cannot be negative.");
            if (item.ReorderLevel < 0)
                throw new ValidationException("Reorder level cannot be negative.");
        }
    }
}
