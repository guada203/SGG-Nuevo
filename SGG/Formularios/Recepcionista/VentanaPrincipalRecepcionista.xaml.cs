using System.Windows;
using System.Windows.Controls;
using SGG.Formularios.Login;

namespace SGG.Formularios.Recepcionista
{
    public partial class VentanaPrincipalRecepcionista : Window
    {
        public VentanaPrincipalRecepcionista()
        {
            InitializeComponent();
            menuLateral.ConfigurarRol("Recepcionista");
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
                "Inicio" => new PanelInicioRecepcionista(),
                "Socios" => new GestionSocios(),
                "Pagos" => new RegistrarPago(),
                "Asistencia" => new ControlAsistencia(),
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