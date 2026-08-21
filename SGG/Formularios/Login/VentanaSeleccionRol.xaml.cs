using System;
using System.Collections.Generic;
using System.Text;
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
    public partial class VentanaSeleccionRol : Window
    {
        public VentanaSeleccionRol()
        {
            InitializeComponent();
        }

        private void Rol_Administrador_Click(object sender, RoutedEventArgs e)
        {
            AbrirLogin("Administrador");
        }

        private void Rol_Recepcionista_Click(object sender, RoutedEventArgs e)
        {
            AbrirLogin("Recepcionista");
        }

        private void Rol_Entrenador_Click(object sender, RoutedEventArgs e)
        {
            AbrirLogin("Entrenador");
        }

        private void AbrirLogin(string rolSeleccionado)
        {
            var ventanaLogin = new VentanaLogin(rolSeleccionado);
            ventanaLogin.Show();
            this.Close();
        }
    }
}