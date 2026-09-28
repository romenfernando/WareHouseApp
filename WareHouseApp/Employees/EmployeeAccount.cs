namespace WareHouseApp.Employees
{
    /// <summary>
    /// A row from the Employees table, for listing/editing in the Employee
    /// Management screen. Deliberately excludes the password hash - the UI
    /// never needs to see it, only replace it via a reset.
    /// </summary>
    public class EmployeeAccount
    {
        public int EmployeeID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
