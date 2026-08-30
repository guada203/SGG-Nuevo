using System;
using System.Collections.ObjectModel;
using System.Windows;
using SGG.Formularios.Login;

namespace SGG.Formularios.Recepcionista
{
    public partial class RegistrarPago : Window
    {
        public ObservableCollection<SocioVista> Socios { get; set; } = new();
        public ObservableCollection<PagoVista> Pagos { get; set; } = new();

        public RegistrarPago()
        {
            InitializeComponent();
            CargarDatosDeEjemplo();
            cmbSocio.ItemsSource = Socios;
            dgPagos.ItemsSource = Pagos;
        }

        private void CargarDatosDeEjemplo()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica / EF Core
            Socios.Add(new SocioVista { Id = 1, NombreCompleto = "Carolina Méndez" });
            Socios.Add(new SocioVista { Id = 2, NombreCompleto = "Tomás Restrepo" });
            Socios.Add(new SocioVista { Id = 3, NombreCompleto = "Lucía Vargas" });

            Pagos.Add(new PagoVista { Socio = "Carolina Méndez", Monto = "$18.000", Fecha = "01/08/2026", Metodo = "Efectivo" });
            Pagos.Add(new PagoVista { Socio = "Tomás Restrepo", Monto = "$15.000", Fecha = "03/08/2026", Metodo = "Tarjeta" });
        }

        private void btnRegistrarPago_Click(object sender, RoutedEventArgs e)
        {
            OcultarError();

            if (cmbSocio.SelectedItem == null)
            {
                MostrarError("Debe seleccionar un socio.");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMonto.Text) || !decimal.TryParse(txtMonto.Text, out decimal monto) || monto <= 0)
            {
                MostrarError("Ingrese un monto válido.");
                return;
            }

            if (cmbMetodoPago.SelectedItem == null)
            {
                MostrarError("Debe seleccionar un método de pago.");
                return;
            }

            // TODO: acá va el registro real del pago contra SGG.Logica / EF Core (RF-05)
            var socio = (SocioVista)cmbSocio.SelectedItem;
            var metodo = ((System.Windows.Controls.ComboBoxItem)cmbMetodoPago.SelectedItem).Content.ToString();

            Pagos.Add(new PagoVista
            {
                Socio = socio.NombreCompleto,
                Monto = $"${monto:N0}",
                Fecha = DateTime.Now.ToString("dd/MM/yyyy"),
                Metodo = metodo ?? ""
            });

            MessageBox.Show("Pago registrado con éxito (simulado).");
            txtMonto.Clear();
            cmbSocio.SelectedIndex = -1;
            cmbMetodoPago.SelectedIndex = -1;
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

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            var dashboard = new VentanaPrincipalRecepcionista();
            dashboard.Show();
            this.Close();
        }

        private void btnSocios_Click(object sender, RoutedEventArgs e)
        {
            var gestionSocios = new GestionSocios();
            gestionSocios.Show();
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

    public class PagoVista
    {
        public string Socio { get; set; } = string.Empty;
        public string Monto { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Metodo { get; set; } = string.Empty;
    }
}