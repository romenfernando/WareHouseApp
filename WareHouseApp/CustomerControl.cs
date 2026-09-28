using System;
using System.Windows.Forms;
using WareHouseApp.Exceptions;
using WareHouseApp.Services;

namespace WareHouseApp
{
    public partial class CustomerControl : UserControl
    {
        private int? selectedCustomerId;

        public CustomerControl()
        {
            InitializeComponent();
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            try
            {
                var items = CustomerService.GetAll();
                dgvCustomers.DataSource = items;

                if (dgvCustomers.Columns["CustomerID"] != null)
                    dgvCustomers.Columns["CustomerID"].Visible = false;
                if (dgvCustomers.Columns["CreatedAt"] != null)
                    dgvCustomers.Columns["CreatedAt"].DefaultCellStyle.Format = "dd/MM/yyyy";
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
                var customer = ReadFormIntoCustomer();
                CustomerService.Add(customer);
                ShowStatus("Customer added.");
                ClearForm();
                RefreshGrid();
            }
            catch (ValidationException ex)
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
            if (selectedCustomerId == null)
            {
                ShowError("Select a customer in the list first.");
                return;
            }

            try
            {
                var customer = ReadFormIntoCustomer();
                customer.CustomerID = selectedCustomerId.Value;
                CustomerService.Update(customer);
                ShowStatus("Customer updated.");
                ClearForm();
                RefreshGrid();
            }
            catch (ValidationException ex)
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
            if (selectedCustomerId == null)
            {
                ShowError("Select a customer in the list first.");
                return;
            }

            var confirm = MessageBox.Show("Delete this customer?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                CustomerService.Delete(selectedCustomerId.Value);
                ShowStatus("Customer deleted.");
                ClearForm();
                RefreshGrid();
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

        private void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCustomers.CurrentRow == null) return;

            var customer = dgvCustomers.CurrentRow.DataBoundItem as WareHouseApp.Customer.Customer;
            if (customer == null) return;

            selectedCustomerId = customer.CustomerID;
            txtName.Text = customer.FullName;
            txtPhone.Text = customer.Phone;
            txtEmail.Text = customer.Email;
            txtAddress.Text = customer.Address;
        }

        private WareHouseApp.Customer.Customer ReadFormIntoCustomer()
        {
            return new WareHouseApp.Customer.Customer
            {
                FullName = txtName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Address = txtAddress.Text.Trim()
            };
        }

        private void ClearForm()
        {
            selectedCustomerId = null;
            txtName.Text = "";
            txtPhone.Text = "";
            txtEmail.Text = "";
            txtAddress.Text = "";
            dgvCustomers.ClearSelection();
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
