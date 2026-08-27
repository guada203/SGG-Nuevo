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

            OcultarError();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MostrarError("Debe completar email y contraseña.");
                return;
            }

            if (!EsEmailValido(email))
            {
                MostrarError("El formato del email no es válido.");
                return;
            }

            if (password.Length < 4)
            {
                MostrarError("La contraseña debe tener al menos 4 caracteres.");
                return;
            }

            var resultado = _servicioAuth.ValidarCredenciales(email, password, _rolSeleccionado);

            if (!resultado.Exitoso)
            {
                MostrarError(resultado.Mensaje);
                return;
            }

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