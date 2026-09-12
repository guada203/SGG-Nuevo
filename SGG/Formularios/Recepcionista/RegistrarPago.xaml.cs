using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SGG.Formularios.Recepcionista
{
    public partial class RegistrarPago : UserControl
    {
        private static readonly CultureInfo EsAr = new("es-AR");

        public ObservableCollection<SocioVista> Socios { get; set; } = new();
        public ObservableCollection<PagoVista> Pagos { get; set; } = new();

        public RegistrarPago()
        {
            InitializeComponent();
            CargarSocios();
            CargarPagos();
            cmbSocio.ItemsSource = Socios;
            dgPagos.ItemsSource = Pagos;
        }

        private void CargarSocios()
        {
            foreach (var s in DatosRecepDemo.ObtenerSocios())
            {
                Socios.Add(new SocioVista
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    EstadoCuota = DatosRecepDemo.EstadoCuota(s)
                });
            }
        }

        private void CargarPagos()
        {
            var pagos = DatosRecepDemo.ObtenerPagos()
                .OrderByDescending(p => p.Fecha)
                .Select(p => new PagoVista
                {
                    Socio = p.SocioNombre,
                    Monto = $"${p.Monto.ToString("N0", EsAr)}",
                    Fecha = p.Fecha.ToString("dd/MM/yyyy"),
                    Metodo = p.Metodo
                });

            foreach (var p in pagos)
                Pagos.Add(p);
        }

        private void btnRegistrarPago_Click(object sender, RoutedEventArgs e)
        {
            OcultarAvisos();

            if (cmbSocio.SelectedItem is not SocioVista socio)
            {
                MostrarError("Debe seleccionar un socio.");
                return;
            }

            // En es-AR el "." es separador de miles y la "," es el decimal:
            // acepta "18000", "18.000" y "18.000,50".
            if (!decimal.TryParse(txtMonto.Text.Trim(), NumberStyles.Number, EsAr, out decimal monto) || monto <= 0)
            {
                MostrarError("Ingrese un monto válido (número decimal mayor a 0).");
                return;
            }

            if (cmbMetodoPago.SelectedItem is not ComboBoxItem metodoItem ||
                string.IsNullOrWhiteSpace(metodoItem.Content?.ToString()))
            {
                MostrarError("Debe seleccionar un método de pago.");
                return;
            }

            // TODO integración BD: acá se registra el pago real contra SGG.Logica / SGG.Datos (RF-05).
            Pagos.Insert(0, new PagoVista
            {
                Socio = socio.NombreCompleto,
                Monto = $"${monto.ToString("N0", EsAr)}",
                Fecha = DateTime.Now.ToString("dd/MM/yyyy"),
                Metodo = metodoItem.Content.ToString() ?? ""
            });

            txtMonto.Clear();
            cmbSocio.SelectedIndex = -1;
            cmbMetodoPago.SelectedIndex = -1;

            txtExito.Text = "Pago registrado correctamente.";
            txtExito.Visibility = Visibility.Visible;
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private void OcultarAvisos()
        {
            txtError.Visibility = Visibility.Collapsed;
            txtExito.Visibility = Visibility.Collapsed;
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