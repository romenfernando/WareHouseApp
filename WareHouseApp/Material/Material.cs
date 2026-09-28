using System;

namespace WareHouseApp.Material
{
    /// <summary>
    /// A single inventory item. Plain data + one small piece of business logic
    /// (IsLowStock) that belongs on the object itself rather than scattered
    /// around the UI - a simple example of encapsulation.
    /// </summary>
    public class Material
    {
        public int MaterialID { get; set; }
        public string MaterialName { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int ReorderLevel { get; set; }
        public DateTime LastUpdated { get; set; }

        public bool IsLowStock
        {
            get { return Quantity <= ReorderLevel; }
        }
    }
}
