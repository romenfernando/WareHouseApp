using WareHouseApp.Security;

namespace WareHouseApp.People
{
    /// <summary>
    /// Base type for anyone who can log into the system. Abstraction: shared
    /// identity fields live here. Inheritance/polymorphism: Admin and
    /// ShippingOperator each implement Login/ChangePassword/Logout their own
    /// way (different DB rules per role) while calling code can work with
    /// them purely through this Person reference.
    /// </summary>
    public abstract class Person
    {
        public int empID;
        public string userName;
        public string fullName;
        public string email;

        private string passwordHash;

        /// <summary>Write-only: assigning to this always stores a salted hash, never plain text.</summary>
        public string Password
        {
            set { passwordHash = PasswordHasher.Hash(value); }
        }

        protected string PasswordHash
        {
            get { return passwordHash; }
            set { passwordHash = value; }
        }

        public abstract string Role { get; }

        public abstract bool Login(string username, string password);
        public abstract void ChangePassword(string newPassword);
        public abstract void Logout(int userID);
    }
}
