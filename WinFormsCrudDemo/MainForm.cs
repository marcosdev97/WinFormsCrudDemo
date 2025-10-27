using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using WinFormsCrudDemo.Data;
using WinFormsCrudDemo.Utils;

namespace WinFormsCrudDemo
{
    public partial class MainForm : Form
    {
        private int? _selectedId = null;
        private bool _isNewMode = false;
        private bool _changingSelection = false;

        public MainForm()
        {
            InitializeComponent();
            btnGuardar.Enabled = false;

            txtNombre.TextChanged += (_, __) => UpdateSaveEnabled();
            txtSalario.TextChanged += (_, __) => UpdateSaveEnabled();
            dtpFechaAlta.ValueChanged += (_, __) => UpdateSaveEnabled();
            CargarTabla();
            dgv.SelectionChanged += Dgv_SelectionChanged;
            txtBuscar.TextChanged += (_, __) => Buscar();
        }

        private void CargarTabla(int? selectId = null)
        {
            _changingSelection = true;
            try
            {
                var dt = Db.GetAll();
                dgv.DataSource = dt;

                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgv.ReadOnly = true;
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgv.MultiSelect = false;

                if (dgv.Columns.Contains("IdEmpleado"))
                {
                    dgv.Columns["IdEmpleado"].HeaderText = "ID";
                }
                if (dgv.Columns.Contains("Salario"))
                {
                    dgv.Columns["Salario"].HeaderText = "Salario (€)";
                    dgv.Columns["Salario"].DefaultCellStyle.Format = "N2";
                }
                if (dgv.Columns.Contains("FechaAlta"))
                {
                    dgv.Columns["FechaAlta"].HeaderText = "Fecha alta";
                    dgv.Columns["FechaAlta"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }

                dgv.ClearSelection();
                if (selectId.HasValue)
                    SeleccionarFilaPorId(selectId.Value);
            }
            catch (Exception ex)
            {
                ShowError("Cargar datos", ex, "UI.CargarTabla");
            }
            finally
            {
                _changingSelection = false;
            }
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
            if (_isNewMode || _changingSelection) return;
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
            try
            {
                _isNewMode = false;
                CargarTabla(_selectedId);  // intenta mantener selección si existe
                MessageBox.Show("Datos actualizados.", "Refrescar",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Refrescar", ex, "UI.btnRefrescar_Click");
            }
        }



        private void btnNuevo_Click(object sender, EventArgs e)
        {
            _isNewMode = true;
            _selectedId = null;
            txtNombre.Clear();
            txtPuesto.Clear();
            txtSalario.Clear();
            dtpFechaAlta.Value = DateTime.Today;

            try { dgv.ClearSelection(); } catch { }

            UpdateSaveEnabled();
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Validación rápida de UI
                if (!ValidateForm())
                {
                    ShowWarn("Validación", "Revisa los campos marcados en rojo.");
                    return;
                }

                // Preparar salario
                decimal? salario = null;
                if (!string.IsNullOrWhiteSpace(txtSalario.Text) &&
                    TryParseDecimal(txtSalario.Text, out var s))
                    salario = s;

                // INSERT o UPDATE
                if (_selectedId == null) // modo nuevo
                {
                    var newId = Db.Insert(
                        txtNombre.Text.Trim(),
                        txtPuesto.Text.Trim(),
                        salario,
                        dtpFechaAlta.Value.Date);

                    _isNewMode = false;
                    CargarTabla(newId);              // recarga y selecciona insertado
                    MessageBox.Show("Registro insertado.", "Guardar",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else // update
                {
                    Db.Update(
                        _selectedId.Value,
                        txtNombre.Text.Trim(),
                        txtPuesto.Text.Trim(),
                        salario,
                        dtpFechaAlta.Value.Date);

                    _isNewMode = false;
                    CargarTabla(_selectedId.Value);  // mantén selección
                    MessageBox.Show("Registro actualizado.", "Guardar",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                UpdateSaveEnabled();
            }
            catch (Exception ex)
            {
                ShowError("Guardar", ex, "UI.btnGuardar_Click");
            }
        }



        private void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedId == null)
                {
                    ShowWarn("Eliminar", "No hay ningún registro seleccionado.");
                    return;
                }

                var confirm = MessageBox.Show(
                    "¿Eliminar el registro seleccionado?",
                    "Confirmar borrado",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes) return;

                Db.Delete(_selectedId.Value);

                
                _selectedId = null;
                CargarTabla();           // recarga estado
                btnNuevo_Click(sender, e); // limpia campos
                _isNewMode = false;

                MessageBox.Show("Registro eliminado.", "Eliminar",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Eliminar", ex, "UI.btnEliminar_Click");
            }
        }

        private void SeleccionarFilaPorId(int id)
        {
            _changingSelection = true;
            try
            {
                dgv.ClearSelection();
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    var drv = row.DataBoundItem as DataRowView;
                    if (drv == null) continue;

                    if (Convert.ToInt32(drv.Row["IdEmpleado"]) == id)
                    {
                        // Selección real
                        row.Selected = true;
                        dgv.CurrentCell = row.Cells[0]; 
                        if (row.Index >= 0) dgv.FirstDisplayedScrollingRowIndex = row.Index;

                        _selectedId = id;
                        txtNombre.Text = drv.Row["Nombre"].ToString();
                        txtPuesto.Text = drv.Row["Puesto"]?.ToString();
                        txtSalario.Text = drv.Row["Salario"] == DBNull.Value ? "" :
                            Convert.ToDecimal(drv.Row["Salario"]).ToString(CultureInfo.InvariantCulture);
                        dtpFechaAlta.Value = Convert.ToDateTime(drv.Row["FechaAlta"]);
                        break;
                    }
                }
            }
            finally
            {
                _changingSelection = false; 
            }
        }

        private bool TryParseDecimal(string text, out decimal value)
        {
            // Acepta coma o punto como separador
            var normalized = (text ?? "").Trim().Replace(",", ".");
            return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private bool ValidateForm()
        {
            bool ok = true;
            errorProvider1.Clear();

            // Nombre obligatorio
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                errorProvider1.SetError(txtNombre, "El nombre es obligatorio.");
                ok = false;
            }

            // Salario: vacío = permitido; si trae algo, debe ser numérico >= 0
            if (!string.IsNullOrWhiteSpace(txtSalario.Text))
            {
                if (!TryParseDecimal(txtSalario.Text, out var s))
                {
                    errorProvider1.SetError(txtSalario, "Debe ser un número (usa 1234.56 o 1234,56).");
                    ok = false;
                }
                else if (s < 0)
                {
                    errorProvider1.SetError(txtSalario, "No puede ser negativo.");
                    ok = false;
                }
            }

            // Fecha coherente: no en el futuro lejano
            if (dtpFechaAlta.Value.Date > DateTime.Today.AddDays(1))
            {
                errorProvider1.SetError(dtpFechaAlta, "Fecha no puede ser futura.");
                ok = false;
            }

            return ok;
        }

        private void UpdateSaveEnabled()
        {
            // Activa Guardar si al menos pasa validaciones mínimas (nombre + salario numérico si lo hay)
            errorProvider1.Clear();
            bool nameOk = !string.IsNullOrWhiteSpace(txtNombre.Text);
            bool salaryOk = string.IsNullOrWhiteSpace(txtSalario.Text) || TryParseDecimal(txtSalario.Text, out _);

            btnGuardar.Enabled = nameOk && salaryOk;
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv.Rows.Count == 0)
                {
                    MessageBox.Show("No hay datos para exportar.", "Exportar CSV",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var sfd = new SaveFileDialog()
                {
                    Filter = "CSV (*.csv)|*.csv",
                    FileName = "Empleados.csv",
                    RestoreDirectory = true
                })
                {
                    if (sfd.ShowDialog() != DialogResult.OK) return;

                    ExportDataGridViewToCsv(dgv, sfd.FileName);

                    var abrir = MessageBox.Show("Exportado correctamente.\n\n¿Quieres abrir el archivo?",
                        "Exportar CSV", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (abrir == DialogResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                            { FileName = sfd.FileName, UseShellExecute = true });
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar: " + ex.Message, "Exportar CSV",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportDataGridViewToCsv(DataGridView grid, string filePath)
        {
            var es = new CultureInfo("es-ES");
            var sb = new StringBuilder();

            // pista para Excel
            sb.AppendLine("sep=;");
            char sep = ';';

            // cabeceras
            bool first;
            first = true;
            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (!col.Visible) continue;
                if (!first) sb.Append(sep);
                sb.Append(E(col.HeaderText ?? ""));
                first = false;
            }
            sb.AppendLine();

            // filas visibles
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow || !row.Visible) continue;

                first = true;
                foreach (DataGridViewColumn col in grid.Columns)
                {
                    if (!col.Visible) continue;

                    var val = row.Cells[col.Index].Value;
                    string text;

                    if (val is DateTime dt)
                        text = dt.ToString("dd/MM/yyyy", es);           // <- fecha corta
                    else if (val is IFormattable && !(val is string))
                        text = ((IFormattable)val).ToString(null, es);  // <- números con coma
                    else
                        text = (val?.ToString() ?? "");

                    text = text.Replace("\r\n", " ").Replace("\n", " ");

                    if (!first) sb.Append(sep);
                    sb.Append(E(text));
                    first = false;
                }
                sb.AppendLine();
            }

            // ANSI (Windows-1252) para que Excel ES no “rompa” acentos
            var ansi = Encoding.GetEncoding(1252);
            File.WriteAllText(filePath, sb.ToString(), ansi);
        }

        // Reemplaza la función local estática por un método privado
        private string E(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";

        private void ShowWarn(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowError(string title, Exception ex, string? extra = null)
        {
            Logger.Log(ex, extra ?? title);
            MessageBox.Show(
                $"Ha ocurrido un error.\n\nDetalles guardados en:\n{Logger.GetLogPath()}",
                title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
