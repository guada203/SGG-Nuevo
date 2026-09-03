using SGG.Formularios.Login;
using SGG.Logica.Servicios;
using System.Windows;

namespace SGG.Formularios.Admin
{
    public partial class VentanaPrincipalAdmin : Window
    {
        private readonly ServicioDashboard _servicioDashboard = new();

        public VentanaPrincipalAdmin()
        {
            InitializeComponent();
            // Suscribirse a los eventos del menú lateral para navegación
            menuLateral.OpcionSeleccionada += ManejarOpcionSeleccionada;

            // Configurar el menú lateral para el rol administrador (muestra los botones correspondientes)
            try
            {
                menuLateral.ConfigurarRol("Administrador");
            }
            catch
            {
                // Evitar que una excepción en la configuración del menú impida que la ventana se muestre
            }

            CargarDatosDashboard();
        }

        private void CargarDatosDashboard()
        {
            txtTotalSocios.Text = _servicioDashboard.ObtenerCantidadSociosActivos().ToString();
            txtTotalUsuarios.Text = _servicioDashboard.ObtenerCantidadUsuarios().ToString();
            txtIngresosMes.Text = _servicioDashboard.ObtenerIngresosDelMes().ToString("C0");
        }

        private void ManejarOpcionSeleccionada(string opcion)
        {
            switch (opcion)
            {
                case "Inicio":
                    // Ya estamos en el dashboard, no hacemos nada
                    break;
                case "Usuarios":
                    var gestionUsuarios = new GestionUsuarios();
                    gestionUsuarios.Show();
                    this.Close();
                    break;
                case "Membresias":
                    var gestionMembresias = new GestionMembresias();
                    gestionMembresias.Show();
                    this.Close();
                    break;
                case "Socios":
                    var gestionSociosAdmin = new GestionSociosAdmin();
                    gestionSociosAdmin.Show();
                    this.Close();
                    break;
                case "Reportes":
                    var reportes = new Reportes();
                    reportes.Show();
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