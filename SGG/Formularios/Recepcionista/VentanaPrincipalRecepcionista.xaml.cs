using SGG.Formularios.Login;
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

namespace SGG.Formularios.Recepcionista
{
    public partial class VentanaPrincipalRecepcionista : Window
    {
        public VentanaPrincipalRecepcionista()
        {
            InitializeComponent();
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            // Ya estamos en el inicio
        }

        private void btnSocios_Click(object sender, RoutedEventArgs e)
        {
            var gestionSocios = new GestionSocios();
            gestionSocios.Show();
            this.Close();
        }

        private void btnPagos_Click(object sender, RoutedEventArgs e)
        {
            var registrarPago = new RegistrarPago();
            registrarPago.Show();
            this.Close();
        }

        private void btnAsistencia_Click(object sender, RoutedEventArgs e)
        {
            var controlAsistencia = new ControlAsistencia();
            controlAsistencia.Show();
            this.Close();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var ventanaRol = new VentanaSeleccionRol();
            ventanaRol.Show();
            this.Close();
        }
    }
}
