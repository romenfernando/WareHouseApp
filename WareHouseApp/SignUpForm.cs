using System;
using System.Windows.Forms;
using WareHouseApp.Exceptions;
using WareHouseApp.Services;

namespace WareHouseApp
{
    public partial class SignUpForm : Form
    {
        public SignUpForm()
        {
            InitializeComponent();
            cmbRole.SelectedIndex = 1; // default to ShippingOperator; Admin is opt-in
        }

        private void btnSignUp_Click(object sender, EventArgs e)
        {
            try
            {
                AccountService.SignUp(
                    txtUsername.Text.Trim(),
                    txtPassword.Text,
                    txtConfirmPassword.Text,
                    txtFullName.Text.Trim(),
                    txtEmail.Text.Trim(),
                    cmbRole.SelectedItem == null ? "" : cmbRole.SelectedItem.ToString());

                MessageBox.Show(
                    "Account created. You can now log in.",
                    "Sign Up Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (ValidationException ex)
            {
                MessageBox.Show(ex.Message, "Check Your Details", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (DuplicateUserException ex)
            {
                MessageBox.Show(ex.Message, "Username Taken", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (DatabaseOperationException ex)
            {
                MessageBox.Show(ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
