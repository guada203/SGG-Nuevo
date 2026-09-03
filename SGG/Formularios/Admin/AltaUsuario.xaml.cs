using System.Windows;
using SGG.Datos.Repositorios;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Admin
{
    public partial class AltaUsuario : Window
    {
        private readonly ServicioUsuarios _servicioUsuarios = new();

        public AltaUsuario()
        {
            InitializeComponent();
            var rolRepositorio = new RolRepositorio();
            cmbRol.ItemsSource = rolRepositorio.ObtenerTodos();
        }

        private void btnCrear_Click(object sender, RoutedEventArgs e)
        {
            OcultarError();

            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string dni = txtDni.Text.Trim();
            string direccion = txtDireccion.Text.Trim();
            string telefono = txtTelefono.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido)
                || string.IsNullOrWhiteSpace(dni) || string.IsNullOrWhiteSpace(email)
                || string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Debe completar los campos obligatorios (Nombre, Apellido, DNI, Email y contraseña).");
                return;
            }

            if (password.Length < 4)
            {
                MostrarError("La contraseña debe tener al menos 4 caracteres.");
                return;
            }

            if (cmbRol.SelectedValue == null)
            {
                MostrarError("Debe seleccionar un rol.");
                return;
            }

            int rolId = (int)cmbRol.SelectedValue;

            var resultado = _servicioUsuarios.AltaUsuario(
                nombre, apellido,
                string.IsNullOrWhiteSpace(direccion) ? null : direccion,
                string.IsNullOrWhiteSpace(telefono) ? null : telefono,
                dni, email, password, rolId);

            if (!resultado.Exitoso)
            {
                MostrarError(resultado.Mensaje);
                return;
            }

            MessageBox.Show(resultado.Mensaje);
            this.Close(); // se cierra y vuelve a GestionUsuarios, que recarga la lista automáticamente
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