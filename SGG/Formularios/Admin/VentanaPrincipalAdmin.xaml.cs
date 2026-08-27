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

namespace SGG.Formularios.Admin
{
    public partial class VentanaPrincipalAdmin : Window
    {
        public VentanaPrincipalAdmin()
        {
            InitializeComponent();
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {

            var reportes = new Reportes();
            reportes.Show();
            this.Close();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            var gestionUsuarios = new GestionUsuarios();
            gestionUsuarios.Show();
            this.Close();
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            var reportes = new Reportes();
            reportes.Show();
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