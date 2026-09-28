using System;
using System.Globalization;
using System.Windows.Forms;
using WareHouseApp.Exceptions;
using WareHouseApp.Services;

namespace WareHouseApp
{
    public partial class InventoryControl : UserControl
    {
        private int? selectedMaterialId;

        public InventoryControl()
        {
            InitializeComponent();
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            try
            {
                var items = InventoryService.GetAll();
                dgvInventory.DataSource = items;

                if (dgvInventory.Columns["MaterialID"] != null)
                    dgvInventory.Columns["MaterialID"].Visible = false;
                if (dgvInventory.Columns["IsLowStock"] != null)
                    dgvInventory.Columns["IsLowStock"].Visible = false;
                if (dgvInventory.Columns["UnitPrice"] != null)
                    dgvInventory.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";

                HighlightLowStockRows();
            }
            catch (DatabaseOperationException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void HighlightLowStockRows()
        {
            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                var material = row.DataBoundItem as WareHouseApp.Material.Material;
                if (material != null && material.IsLowStock)
                {
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.MistyRose;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var item = ReadFormIntoMaterial();
                InventoryService.Add(item);
                ShowStatus("Item added.");
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
            if (selectedMaterialId == null)
            {
                ShowError("Select an item in the list first.");
                return;
            }

            try
            {
                var item = ReadFormIntoMaterial();
                item.MaterialID = selectedMaterialId.Value;
                InventoryService.Update(item);
                ShowStatus("Item updated.");
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
            if (selectedMaterialId == null)
            {
                ShowError("Select an item in the list first.");
                return;
            }

            var confirm = MessageBox.Show("Delete this item?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                InventoryService.Delete(selectedMaterialId.Value);
                ShowStatus("Item deleted.");
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

        private void dgvInventory_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvInventory.CurrentRow == null) return;

            var material = dgvInventory.CurrentRow.DataBoundItem as WareHouseApp.Material.Material;
            if (material == null) return;

            selectedMaterialId = material.MaterialID;
            txtName.Text = material.MaterialName;
            txtCategory.Text = material.Category;
            txtQuantity.Text = material.Quantity.ToString(CultureInfo.InvariantCulture);
            txtUnitPrice.Text = material.UnitPrice.ToString(CultureInfo.InvariantCulture);
            txtReorderLevel.Text = material.ReorderLevel.ToString(CultureInfo.InvariantCulture);
        }

        private WareHouseApp.Material.Material ReadFormIntoMaterial()
        {
            int quantity;
            if (!int.TryParse(txtQuantity.Text.Trim(), out quantity))
                throw new ValidationException("Quantity must be a whole number.");

            decimal unitPrice;
            if (!decimal.TryParse(txtUnitPrice.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out unitPrice))
                throw new ValidationException("Unit price must be a number.");

            int reorderLevel;
            if (!int.TryParse(txtReorderLevel.Text.Trim(), out reorderLevel))
                throw new ValidationException("Reorder level must be a whole number.");

            return new WareHouseApp.Material.Material
            {
                MaterialName = txtName.Text.Trim(),
                Category = txtCategory.Text.Trim(),
                Quantity = quantity,
                UnitPrice = unitPrice,
                ReorderLevel = reorderLevel
            };
        }

        private void ClearForm()
        {
            selectedMaterialId = null;
            txtName.Text = "";
            txtCategory.Text = "";
            txtQuantity.Text = "";
            txtUnitPrice.Text = "";
            txtReorderLevel.Text = "";
            dgvInventory.ClearSelection();
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
