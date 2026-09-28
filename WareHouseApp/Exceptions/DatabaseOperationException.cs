using System;

namespace WareHouseApp.Exceptions
{
    /// <summary>
    /// Wraps low-level SqlException/database failures into a friendly, app-specific
    /// exception so the UI layer never needs to know about SqlClient directly.
    /// </summary>
    public class DatabaseOperationException : Exception
    {
        public DatabaseOperationException(string message, Exception inner) : base(message, inner) { }
    }
}
