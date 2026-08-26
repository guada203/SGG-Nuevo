using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SGG.Formularios.Login
{
    public partial class VentanaLogin : Window
    {
        private readonly string _rolSeleccionado;

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

            // ⚠️ TEMPORAL - SACAR ESTO CUANDO CONECTEMOS LA BASE DE DATOS DE VERDAD (Etapa 2)
            // var resultado = _servicioAuth.ValidarCredenciales(email, password, _rolSeleccionado);
            // if (!resultado.Exitoso)
            // {
            //     MostrarError(resultado.Mensaje);
            //     return;
            // }

            if (_rolSeleccionado == "Administrador")
            {
                var ventanaAdmin = new SGG.Formularios.Admin.VentanaPrincipalAdmin();
                ventanaAdmin.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show($"Login simulado OK - Rol: {_rolSeleccionado}");
            }

            // TODO: acá va la validación real contra SGG.Logica / SGG.Datos (RF-21)
            MessageBox.Show($"Validaciones OK - Login simulado\nRol: {_rolSeleccionado}\nEmail: {email}");
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