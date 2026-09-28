using System;

namespace WareHouseApp.Exceptions
{
    /// <summary>
    /// Thrown during Sign Up when the chosen username is already taken.
    /// </summary>
    public class DuplicateUserException : Exception
    {
        public DuplicateUserException(string message) : base(message) { }
    }
}
