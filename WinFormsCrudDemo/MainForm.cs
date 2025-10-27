using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using WinFormsCrudDemo.Data;

namespace WinFormsCrudDemo
{
    public partial class MainForm : Form
    {
        private int? _selectedId = null;
        private bool _isNewMode = false;


        public MainForm()
        {
            InitializeComponent();
            CargarTabla();
            dgv.SelectionChanged += Dgv_SelectionChanged;
            txtBuscar.TextChanged += (_, __) => Buscar();
        }

        private void CargarTabla()
        {
            dgv.DataSource = Db.GetAll();
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
                dgv.DataSource = Db.GetAll();
            else
                dgv.DataSource = Db.SearchByName(txtBuscar.Text.Trim());
        }

        private void Dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (_isNewMode) return;
            if (dgv.CurrentRow == null) return;
            var row = (dgv.CurrentRow.DataBoundItem as DataRowView)?.Row;
            if (row == null) return;

            _selectedId = Convert.ToInt32(row["IdEmpleado"]);
            txtNombre.Text = row["Nombre"].ToString();
            txtPuesto.Text = row["Puesto"]?.ToString();
            txtSalario.Text = row["Salario"] == DBNull.Value ? "" :
                Convert.ToDecimal(row["Salario"]).ToString(CultureInfo.InvariantCulture);
            dtpFechaAlta.Value = Convert.ToDateTime(row["FechaAlta"]);
        }

        private void btnRefrescar_Click(object sender, EventArgs e)
        {
            _isNewMode = false;
            CargarTabla();
        }


        private void btnNuevo_Click(object sender, EventArgs e)
        {
            _isNewMode = true;
            _selectedId = null;
            txtNombre.Clear();
            txtPuesto.Clear();
            txtSalario.Clear();
            dtpFechaAlta.Value = DateTime.Today;

            try { dgv.ClearSelection(); } catch { /* no pasa nada */ }
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Nombre es obligatorio."); return;
            }

            decimal? salario = null;
            if (decimal.TryParse(txtSalario.Text.Replace(",", "."),
                NumberStyles.Any, CultureInfo.InvariantCulture, out var s))
                salario = s;

            if (_selectedId == null)
            {
                var newId = Db.Insert(txtNombre.Text.Trim(), txtPuesto.Text.Trim(), salario, dtpFechaAlta.Value.Date);
                _isNewMode = false;
                CargarTabla();
                SeleccionarFilaPorId(newId); // <-- nuevo
                MessageBox.Show("Registro insertado.");
            }
            else
            {
                Db.Update(_selectedId.Value, txtNombre.Text.Trim(), txtPuesto.Text.Trim(), salario, dtpFechaAlta.Value.Date);
                _isNewMode = false;
                CargarTabla();
                SeleccionarFilaPorId(_selectedId.Value);
                MessageBox.Show("Registro actualizado.");
            }

            CargarTabla();
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_selectedId == null) return;
            if (MessageBox.Show("¿Eliminar registro seleccionado?", "Confirmar",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Db.Delete(_selectedId.Value);
                btnNuevo_Click(sender, e);
                _isNewMode = false;
                CargarTabla();
            }
        }
        private void SeleccionarFilaPorId(int id)
        {
            if (dgv.DataSource is DataTable dt)
            {
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    if (row.DataBoundItem is DataRowView drv && Convert.ToInt32(drv.Row["IdEmpleado"]) == id)
                    {
                        row.Selected = true;
                        dgv.CurrentCell = row.Cells[0];
                        dgv.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
        }

    }
}
