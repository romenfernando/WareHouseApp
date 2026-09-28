using System;

namespace WareHouseApp.Exceptions
{
    /// <summary>
    /// Thrown when user-entered data fails validation (e.g. empty required field,
    /// passwords that don't match, an invalid quantity). Used across features
    /// (sign up, inventory, customers, employees) so the UI can handle all
    /// "bad input" cases the same way.
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
}
