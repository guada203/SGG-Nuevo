using System.Text.RegularExpressions;
using System.Windows;
using SGG.Logica.Servicios;
using SGG.Formularios.Admin;

namespace SGG.Formularios.Login
{
    public partial class VentanaLogin : Window
    {
        private readonly string _rolSeleccionado;
        private readonly ServicioAutenticacion _servicioAuth = new();

        public VentanaLogin(string rolSeleccionado)
        {
            InitializeComponent();
            _rolSeleccionado = rolSeleccionado;
            txtRolSeleccionado.Text = $"Ingresando como {_rolSeleccionado}";
        }

        private void btnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Password;

            // Limpiar error previo
            OcultarError();

            // Validación 1: campos vacíos
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Debe completar email y contraseña.");
                return;
            }

            // Validación 2: formato de email válido
            if (!EsEmailValido(email))
            {
                MostrarError("El formato del email no es válido.");
                return;
            }

            // Validación 3: longitud mínima de contraseña
            if (password.Length < 4)
            {
                MostrarError("La contraseña debe tener al menos 4 caracteres.");
                return;
            }

            // Validación real contra la base de datos (RF-21, RF-22)
            var resultado = _servicioAuth.ValidarCredenciales(email, password, _rolSeleccionado);

            if (!resultado.Exitoso)
            {
                MostrarError(resultado.Mensaje);
                return;
            }

            // Login exitoso: navegar según el rol
            if (_rolSeleccionado == "Administrador")
            {
                var ventanaAdmin = new VentanaPrincipalAdmin();
                ventanaAdmin.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show($"¡Bienvenido {resultado.Usuario!.Nombre}! (Rol: {_rolSeleccionado})");
            }
        }

        private bool EsEmailValido(string email)
        {
            string patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, patron);
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

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            var ventanaRol = new VentanaSeleccionRol();
            ventanaRol.Show();
            this.Close();
        }
    }
}