using System.Windows;
using SGG.Formularios.Login;

namespace SGG.Formularios.Recepcionista
{
    public partial class VentanaPrincipalRecepcionista : Window
    {
        public VentanaPrincipalRecepcionista()
        {
            InitializeComponent();
            menuLateral.OpcionSeleccionada += ManejarOpcionSeleccionada;
            try
            {
                menuLateral.ConfigurarRol("Recepcionista");
            }
            catch
            {
                // Ignorar errores de configuración del menú
            }
        }

        private void ManejarOpcionSeleccionada(string opcion)
        {
            switch (opcion)
            {
                case "Inicio":
                    // Ya estamos acá, no hacemos nada
                    break;
                case "Socios":
                    var gestionSocios = new GestionSocios();
                    gestionSocios.Show();
                    this.Close();
                    break;
                case "Pagos":
                    var registrarPago = new RegistrarPago();
                    registrarPago.Show();
                    this.Close();
                    break;
                case "Asistencia":
                    var controlAsistencia = new ControlAsistencia();
                    controlAsistencia.Show();
                    this.Close();
                    break;
                case "CerrarSesion":
                    var ventanaRol = new VentanaSeleccionRol();
                    ventanaRol.Show();
                    this.Close();
                    break;
            }
        }
    }
}
