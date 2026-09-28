using System;
using System.Windows.Forms;
using WareHouseApp.People;

namespace WareHouseApp
{
    public partial class DashBoard : Form
    {
        private readonly Person loggedInUser;
        private Control currentContent;

        // Kept so the Windows Forms designer can still open this form at
        // design time - not used at runtime.
        public DashBoard()
        {
            InitializeComponent();
        }

        public DashBoard(Person loggedInUser)
        {
            InitializeComponent();
            this.loggedInUser = loggedInUser;
            currentContent = mainDash1;
            ApplyUserContext();
        }

        private void ApplyUserContext()
        {
            if (loggedInUser == null) return;

            button9.Text = "Hi, " + loggedInUser.fullName + " (" + loggedInUser.Role + ")";

            // Employee management (button4) is Admin-only.
            bool isAdmin = loggedInUser.Role == "Admin";
            button4.Visible = isAdmin;
            button4.Enabled = isAdmin;

            button1.Text = "Dashboard";
            button2.Text = "Inventory";
            button3.Text = "Customers";
            button4.Text = "Employees";
            button5.Text = "Reports";
            button6.Text = "Logout";

            button1.Click += (s, e) => ShowContent(mainDash1);
            button2.Click += (s, e) => ShowContent(new InventoryControl());
            button3.Click += (s, e) => ShowContent(new CustomerControl());
            button4.Click += (s, e) => ShowContent(new EmployeeControl(loggedInUser.empID));
            button6.Click += button6_Click;
        }

        /// <summary>
        /// Swaps whatever is currently in the main content area for a new
        /// control, in the same spot mainDash1 originally occupied. Every
        /// feature screen (Inventory, Customers, Employees...) is just a
        /// UserControl handed to this method.
        /// </summary>
        private void ShowContent(Control content)
        {
            if (currentContent != null && currentContent != content)
            {
                this.Controls.Remove(currentContent);
            }

            content.Location = mainDash1.Location;
            content.Size = mainDash1.Size;

            if (!this.Controls.Contains(content))
            {
                this.Controls.Add(content);
            }
            content.BringToFront();
            currentContent = content;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var loginForm = new Form1();
            loginForm.Show();
            this.Close();
        }
    }
}
