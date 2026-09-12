using System.Windows;
using System.Windows.Controls;
using SGG.Formularios.Login;

namespace SGG.Formularios.Admin
{
    public partial class VentanaPrincipalAdmin : Window
    {
        public VentanaPrincipalAdmin()
        {
            InitializeComponent();
            menuLateral.ConfigurarRol("Administrador");
            menuLateral.OpcionSeleccionada += ManejarOpcionSeleccionada;
            MostrarContenido("Inicio");
        }

        private void ManejarOpcionSeleccionada(string opcion)
        {
            if (opcion == "CerrarSesion")
            {
                Sesion.Limpiar();
                var ventanaRol = new VentanaSeleccionRol();
                ventanaRol.Show();
                Close();
                return;
            }
            MostrarContenido(opcion);
        }

        private void MostrarContenido(string opcion)
        {
            UserControl? control = opcion switch
            {
                "Inicio" => new PanelInicioAdmin(),
                "Usuarios" => new GestionUsuarios(),
                "Membresias" => new GestionMembresias(),
                "Socios" => new GestionSociosAdmin(),
                "Reportes" => new Reportes(),
                _ => null
            };
            if (control != null)
            {
                contenido.Content = control;
                menuLateral.SeccionActiva = opcion;
            }
        }
    }
}