using System;
using System.Windows;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Recepcionista
{
    public partial class AltaSocio : Window
    {
        private readonly ServicioSocios _servicioSocios = new();
        private readonly ServicioMembresias _servicioMembresias = new();

        public AltaSocio()
        {
            InitializeComponent();
            CargarMembresias();
        }

        private void CargarMembresias()
        {
            var vigentes = _servicioMembresias.ObtenerVigentes();

            cmbMembresia.Items.Clear();
            foreach (var m in vigentes)
            {
                cmbMembresia.Items.Add(new MembresiaItem
                {
                    Id = m.Id,
                    Descripcion = $"{m.TipoActividad} - {m.Precio:C} (vence {m.FechaVencimiento.ToShortDateString()})"
                });
            }

            if (cmbMembresia.Items.Count > 0)
                cmbMembresia.SelectedIndex = 0;
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            OcultarError();

            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string dni = txtDni.Text.Trim();
            string telefono = txtTelefono.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido)
                || string.IsNullOrWhiteSpace(dni))
            {
                MostrarError("Debe completar los campos obligatorios (Nombre, Apellido y DNI).");
                return;
            }

            DateTime fechaNacimiento = dpNacimiento.SelectedDate ?? DateTime.Today;

            if (cmbMembresia.SelectedItem is not MembresiaItem membresia)
            {
                MostrarError("No hay membresías vigentes cargadas. Contacte al administrador.");
                return;
            }

            var resultado = _servicioSocios.AltaSocio(
                nombre, apellido, dni, fechaNacimiento,
                telefono, email, membresia.Id);

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

    public class MembresiaItem
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
