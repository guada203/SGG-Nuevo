using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using SGG.Formularios.Login;

namespace SGG.Formularios.Recepcionista
{
    public partial class ControlAsistencia : Window
    {
        public ObservableCollection<SocioVista> Socios { get; set; } = new();
        public ObservableCollection<AsistenciaVista> Asistencias { get; set; } = new();

        public ControlAsistencia()
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
            CargarDatosDeEjemplo();
            cmbSocio.ItemsSource = Socios;
            dgAsistencias.ItemsSource = Asistencias;
        }

        private void CargarDatosDeEjemplo()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica / EF Core
            Socios.Add(new SocioVista { Id = 1, NombreCompleto = "Carolina Méndez" });
            Socios.Add(new SocioVista { Id = 2, NombreCompleto = "Tomás Restrepo" });
            Socios.Add(new SocioVista { Id = 3, NombreCompleto = "Lucía Vargas" }); // esta socia está "Inactiva" en el ejemplo

            Asistencias.Add(new AsistenciaVista { Socio = "Carolina Méndez", Hora = "08:42", EstadoCuota = "Al día" });
            Asistencias.Add(new AsistenciaVista { Socio = "Tomás Restrepo", Hora = "18:10", EstadoCuota = "Al día" });
        }

        private void btnRegistrarIngreso_Click(object sender, RoutedEventArgs e)
        {
            OcultarMensaje();

            if (cmbSocio.SelectedItem == null)
            {
                MostrarMensaje("Debe seleccionar un socio.", esError: true);
                return;
            }

            var socio = (SocioVista)cmbSocio.SelectedItem;

            // RF-09: validar cuota al día antes de permitir el ingreso
            // TODO: reemplazar esta validación simulada por la real contra Membresia.FechaVencimiento
            bool cuotaAlDia = socio.NombreCompleto != "Lucía Vargas"; // simulación: Lucía tiene la cuota vencida

            if (!cuotaAlDia)
            {
                MostrarMensaje($"⚠ {socio.NombreCompleto} tiene la cuota vencida. No se permite el ingreso.", esError: true);
                return;
            }

            // RF-08: registrar el ingreso
            Asistencias.Add(new AsistenciaVista
            {
                Socio = socio.NombreCompleto,
                Hora = DateTime.Now.ToString("HH:mm"),
                EstadoCuota = "Al día"
            });

            MostrarMensaje($"✔ Ingreso registrado para {socio.NombreCompleto}.", esError: false);
            cmbSocio.SelectedIndex = -1;
        }

        private void MostrarMensaje(string texto, bool esError)
        {
            txtMensaje.Text = texto;
            txtMensaje.Foreground = esError
                ? new SolidColorBrush(Colors.OrangeRed)
                : new SolidColorBrush(Colors.LightGreen);
            txtMensaje.Visibility = Visibility.Visible;
        }

        private void OcultarMensaje()
        {
            txtMensaje.Visibility = Visibility.Collapsed;
        }

        private void ManejarOpcionSeleccionada(string opcion)
        {
            switch (opcion)
            {
                case "Inicio":
                    var dashboard = new VentanaPrincipalRecepcionista();
                    dashboard.Show();
                    this.Close();
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
                    // Ya estamos acá, no hacemos nada
                    break;
                case "CerrarSesion":
                    var ventanaRol = new VentanaSeleccionRol();
                    ventanaRol.Show();
                    this.Close();
                    break;
            }
        }
    }

    public class AsistenciaVista
    {
        public string Socio { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string EstadoCuota { get; set; } = string.Empty;
    }
}