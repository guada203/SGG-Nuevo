using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SGG.Dominio.Entidades;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Admin
{
    public partial class AltaMembresia : Window
    {
        private readonly ServicioMembresias _servicioMembresias = new();
        private readonly int? _membresiaId;

        public AltaMembresia()
        {
            InitializeComponent();
            txtTitulo.Text = "NUEVA MEMBRESÍA";
            btnGuardar.Content = "GUARDAR MEMBRESÍA";
            cmbActividad.SelectedIndex = 0;
            // Defectos visibles: el usuario los ve y puede cambiarlos (nunca fechas vacías silenciosas).
            dpInicio.SelectedDate = DateTime.Today;
            dpVencimiento.SelectedDate = DateTime.Today.AddMonths(1);
        }

        public AltaMembresia(int id) : this()
        {
            _membresiaId = id;
            txtTitulo.Text = "EDITAR MEMBRESÍA";
            btnGuardar.Content = "ACTUALIZAR MEMBRESÍA";
            CargarMembresia(id);
        }

        private void CargarMembresia(int id)
        {
            var m = _servicioMembresias.ObtenerPorId(id);
            if (m == null)
            {
                MostrarError("No se encontró la membresía.");
                return;
            }

            cmbActividad.SelectedIndex = m.TipoActividad switch
            {
                TipoActividad.Funcional => 1,
                TipoActividad.Combinado => 2,
                _ => 0
            };

            txtPrecio.Text = m.Precio.ToString(CultureInfo.InvariantCulture);
            dpInicio.SelectedDate = m.FechaInicio;
            dpVencimiento.SelectedDate = m.FechaVencimiento;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            OcultarError();

            if (cmbActividad.SelectedItem is not ComboBoxItem item)
            {
                MostrarError("Debe seleccionar una actividad.");
                return;
            }

            var tipoActividad = (TipoActividad)Enum.Parse(typeof(TipoActividad), (string)item.Tag);

            if (string.IsNullOrWhiteSpace(txtPrecio.Text))
            {
                MostrarError("Debe ingresar el precio.");
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal precio))
            {
                MostrarError("El precio debe ser un número válido.");
                return;
            }

            if (precio <= 0)
            {
                MostrarError("El precio debe ser mayor que cero.");
                return;
            }

            if (dpInicio.SelectedDate == null)
            {
                MostrarError("Debe seleccionar la fecha de inicio.");
                return;
            }

            if (dpVencimiento.SelectedDate == null)
            {
                MostrarError("Debe seleccionar la fecha de vencimiento.");
                return;
            }

            DateTime fechaInicio = dpInicio.SelectedDate.Value;
            DateTime fechaVencimiento = dpVencimiento.SelectedDate.Value;

            if (fechaVencimiento <= fechaInicio)
            {
                MostrarError("La fecha de vencimiento debe ser posterior a la fecha de inicio.");
                return;
            }

            var resultado = _membresiaId.HasValue
                ? _servicioMembresias.EditarMembresia(_membresiaId.Value, tipoActividad, precio, fechaInicio, fechaVencimiento)
                : _servicioMembresias.AltaMembresia(tipoActividad, precio, fechaInicio, fechaVencimiento);

            if (!resultado.Exitoso)
            {
                MostrarError(resultado.Mensaje);
                return;
            }

            MessageBox.Show(resultado.Mensaje);
            this.DialogResult = true;
            this.Close();
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private void OcultarError()
        {
            txtError.Visibility = Visibility.Collapsed;
        }
    }
}
