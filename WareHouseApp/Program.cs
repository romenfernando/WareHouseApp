using System;
using System.Windows.Forms;
using WareHouseApp.Database;
using WareHouseApp.Exceptions;

namespace WareHouseApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // |DataDirectory| in the connection string resolves to this folder,
            // so the app finds WarehouseDB.mdf next to the .exe.
            AppDomain.CurrentDomain.SetData("DataDirectory", AppDomain.CurrentDomain.BaseDirectory);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                DbInitializer.EnsureCreated();
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(
                    ex.Message + Environment.NewLine + Environment.NewLine +
                    "Make sure SQL Server LocalDB is installed and try again.",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Start on the login screen (previously this launched straight into
            // the DashBoard, which skipped authentication entirely).
            Application.Run(new Form1());
        }
    }
}
