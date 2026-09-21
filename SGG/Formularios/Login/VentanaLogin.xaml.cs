using SGG.Formularios.Admin;
using SGG.Logica.Servicios;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using System.Windows;

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

        private void btnVerPassword_Click(object sender, RoutedEventArgs e)
        {
            bool mostrandoClaro = txtPasswordVisible.Visibility == Visibility.Visible;

            if (!mostrandoClaro)
            {
                // Pasar lo tipeado al TextBox y mostrar en claro.
                txtPasswordVisible.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPasswordVisible.Focus();
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
                btnVerPassword.ToolTip = "Ocultar contraseña";
            }
            else
            {
                // Devolver el texto al PasswordBox y ocultar.
                txtPassword.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                txtPassword.Focus();
                btnVerPassword.ToolTip = "Ver contraseña";
            }
        }

        private void txtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Mantener sincronizado el PasswordBox oculto mientras se escribe en claro,
            // para que btnIngresar siempre lea el valor correcto desde txtPassword.Password.
            txtPassword.Password = txtPasswordVisible.Text;
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

            Sesion.Iniciar(resultado.Usuario!);

            if (_rolSeleccionado == "Administrador")
            {
                var ventanaAdmin = new VentanaPrincipalAdmin();
                ventanaAdmin.Show();
                this.Close();
            }
            else if (_rolSeleccionado == "Recepcionista")
            {
                var ventanaRecepcionista = new SGG.Formularios.Recepcionista.VentanaPrincipalRecepcionista();
                ventanaRecepcionista.Show();
                this.Close();
            }
            else if (_rolSeleccionado == "Entrenador")
            {
                var ventanaEntrenador = new SGG.Formularios.Entrenador.VentanaPrincipalEntrenador();
                ventanaEntrenador.Show();
                this.Close();
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