using System;
using System.Text.RegularExpressions;
using System.Windows;
using SGG.Datos.Repositorios;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Admin
{
    public partial class AltaUsuario : Window
    {
        private readonly ServicioUsuarios _servicioUsuarios = new();
        private readonly int? _usuarioId;

        public AltaUsuario()
        {
            InitializeComponent();

            Validadores.SoloNumeros(txtDni);
            Validadores.SoloNumeros(txtTelefono);
            Validadores.SoloLetras(txtNombre);
            Validadores.SoloLetras(txtApellido);

            var rolRepositorio = new RolRepositorio();
            cmbRol.ItemsSource = rolRepositorio.ObtenerTodos();
        }

        public AltaUsuario(int id) : this()
        {
            _usuarioId = id;
            txtTitulo.Text = "EDITAR USUARIO";
            btnGuardar.Content = "ACTUALIZAR USUARIO";
            CargarUsuario(id);
        }

        private void CargarUsuario(int id)
        {
            var u = _servicioUsuarios.ObtenerPorId(id);
            if (u == null)
            {
                MostrarError("No se encontró el usuario.");
                return;
            }

            txtNombre.Text = u.Nombre;
            txtApellido.Text = u.Apellido ?? string.Empty;
            txtDni.Text = u.Dni ?? string.Empty;
            txtDireccion.Text = u.Direccion ?? string.Empty;
            txtTelefono.Text = u.Telefono ?? string.Empty;
            txtEmail.Text = u.Email;
            cmbRol.SelectedValue = u.RolId;
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

            bool esEdicion = _usuarioId.HasValue;

            // Al editar la contraseña es opcional (si queda vacía se conserva la actual);
            // al crear sí es obligatoria.
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido)
                || string.IsNullOrWhiteSpace(dni) || string.IsNullOrWhiteSpace(email))
            {
                MostrarError("Debe completar los campos obligatorios (Nombre, Apellido, DNI y Email).");
                return;
            }

            if (!esEdicion && string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Debe completar los campos obligatorios (Nombre, Apellido, DNI, Email y contraseña).");
                return;
            }

            if (!esEdicion && password.Length < 4)
            {
                MostrarError("La contraseña debe tener al menos 4 caracteres.");
                return;
            }

            if (cmbRol.SelectedValue == null)
            {
                MostrarError("Debe seleccionar un rol.");
                return;
            }

            // Validaciones RF-18
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarError("El email ingresado no es válido.");
                return;
            }

            if (!Regex.IsMatch(dni, @"^\d{7,8}$"))
            {
                MostrarError("El DNI debe contener solo números (7 u 8 dígitos).");
                return;
            }

            if (!string.IsNullOrWhiteSpace(telefono) && !Regex.IsMatch(telefono, @"^\d+$"))
            {
                MostrarError("El teléfono debe contener solo números.");
                return;
            }

            int rolId = (int)cmbRol.SelectedValue;

            (bool Exitoso, string Mensaje) resultado;

            if (esEdicion)
            {
                resultado = _servicioUsuarios.EditarUsuario(
                    _usuarioId.Value,
                    nombre, apellido,
                    string.IsNullOrWhiteSpace(direccion) ? null : direccion,
                    string.IsNullOrWhiteSpace(telefono) ? null : telefono,
                    dni, email, rolId,
                    string.IsNullOrWhiteSpace(password) ? null : password);
            }
            else
            {
                resultado = _servicioUsuarios.AltaUsuario(
                    nombre, apellido,
                    string.IsNullOrWhiteSpace(direccion) ? null : direccion,
                    string.IsNullOrWhiteSpace(telefono) ? null : telefono,
                    dni, email, password, rolId);
            }

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