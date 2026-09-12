using System.Windows;
using System.Windows.Controls;
using SGG.Formularios.Login;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaPrincipalEntrenador : Window
    {
        public VentanaPrincipalEntrenador()
        {
            InitializeComponent();
            menuLateral.ConfigurarRol("Entrenador");
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
                "Inicio" => new PanelInicioEntrenador(),
                "Rutinas" => new VentanaListaRutinas(),
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