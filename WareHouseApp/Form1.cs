using System;
using System.Windows.Forms;
using WareHouseApp.Exceptions;
using WareHouseApp.People;
using WareHouseApp.Resources.AppStrings;
using WareHouseApp.Services;

namespace WareHouseApp
{
    public partial class Form1 : Form
    {
        private readonly LoginPage loginPage = new LoginPage();

        public Form1()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void butLogin_Click(object sender, EventArgs e)
        {
            string usernameText = txtName.Text.Trim();
            string passwordText = txtPassword.Text;

            try
            {
                // AccountService.Authenticate works out whether this is an Admin
                // or a ShippingOperator and hands back the right Person subtype -
                // Form1 doesn't need to know which.
                Person loggedInUser = AccountService.Authenticate(usernameText, passwordText);

                DashBoard dashBoard = new DashBoard(loggedInUser);
                dashBoard.Show();
                this.Hide();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Check Your Details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (AuthenticationException)
            {
                MessageBox.Show(
                    loginPage.LoginErrorMessageEn, loginPage.LoginErrorTitleEn,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                txtPassword.Text = "";
            }
        }

        private void linkSignUp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var signUpForm = new SignUpForm())
            {
                signUpForm.ShowDialog(this);
            }
        }
    }
}
