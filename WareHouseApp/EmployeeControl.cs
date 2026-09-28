using System;
using System.Windows.Forms;
using WareHouseApp.Exceptions;
using WareHouseApp.Services;

namespace WareHouseApp
{
    public partial class EmployeeControl : UserControl
    {
        private readonly int currentAdminId;
        private int? selectedEmployeeId;

        public EmployeeControl(int currentAdminId)
        {
            InitializeComponent();
            this.currentAdminId = currentAdminId;
            cmbRole.SelectedIndex = 1;
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            try
            {
                var items = EmployeeService.GetAll();
                dgvEmployees.DataSource = items;

                if (dgvEmployees.Columns["EmployeeID"] != null)
                    dgvEmployees.Columns["EmployeeID"].Visible = false;
            }
            catch (DatabaseOperationException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var employee = ReadFormIntoEmployee();
                EmployeeService.Add(employee, txtNewPassword.Text, txtConfirmPassword.Text);
                ShowStatus("Employee added.");
                ClearForm();
                RefreshGrid();
            }
            catch (ValidationException ex)
            {
                ShowError(ex.Message);
            }
            catch (DuplicateUserException ex)
            {
                ShowError(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedEmployeeId == null)
            {
                ShowError("Select an employee in the list first.");
                return;
            }

            try
            {
                var employee = ReadFormIntoEmployee();
                employee.EmployeeID = selectedEmployeeId.Value;
                EmployeeService.Update(employee, txtNewPassword.Text, txtConfirmPassword.Text);
                ShowStatus("Employee updated.");
                ClearForm();
                RefreshGrid();
            }
            catch (ValidationException ex)
            {
                ShowError(ex.Message);
            }
            catch (DuplicateUserException ex)
            {
                ShowError(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedEmployeeId == null)
            {
                ShowError("Select an employee in the list first.");
                return;
            }

            var confirm = MessageBox.Show("Delete this employee account?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                EmployeeService.Delete(selectedEmployeeId.Value, currentAdminId);
                ShowStatus("Employee deleted.");
                ClearForm();
                RefreshGrid();
            }
            catch (ValidationException ex)
            {
                // Thrown when trying to delete your own account.
                ShowError(ex.Message);
            }
            catch (DatabaseOperationException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void dgvEmployees_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null) return;

            var employee = dgvEmployees.CurrentRow.DataBoundItem as WareHouseApp.Employees.EmployeeAccount;
            if (employee == null) return;

            selectedEmployeeId = employee.EmployeeID;
            txtUsername.Text = employee.Username;
            txtFullName.Text = employee.FullName;
            txtEmail.Text = employee.Email;
            cmbRole.SelectedItem = employee.Role;

            // Never pre-fill password fields - blank means "keep existing".
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";
        }

        private WareHouseApp.Employees.EmployeeAccount ReadFormIntoEmployee()
        {
            return new WareHouseApp.Employees.EmployeeAccount
            {
                Username = txtUsername.Text.Trim(),
                FullName = txtFullName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Role = cmbRole.SelectedItem == null ? "" : cmbRole.SelectedItem.ToString()
            };
        }

        private void ClearForm()
        {
            selectedEmployeeId = null;
            txtUsername.Text = "";
            txtFullName.Text = "";
            txtEmail.Text = "";
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";
            cmbRole.SelectedIndex = 1;
            dgvEmployees.ClearSelection();
        }

        private void ShowStatus(string message)
        {
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(5, 146, 18);
            lblStatus.Text = message;
        }

        private void ShowError(string message)
        {
            lblStatus.ForeColor = System.Drawing.Color.FromArgb(180, 40, 40);
            lblStatus.Text = message;
        }
    }
}
