using System;

namespace WareHouseApp.Exceptions
{
    /// <summary>
    /// Thrown when a login attempt fails (unknown user, wrong password, or wrong role).
    /// Keeping this as its own type lets the UI show a friendly, specific message
    /// instead of a generic "something went wrong".
    /// </summary>
    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
        public AuthenticationException(string message, Exception inner) : base(message, inner) { }
    }
}
