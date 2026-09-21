using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SGG.Formularios.Recepcionista
{
    public partial class RegistrarPago : UserControl
    {
        private static readonly CultureInfo EsAr = new("es-AR");

        private readonly List<SocioDemo> _sociosDemo = DatosRecepDemo.ObtenerSocios();
        private List<SocioVista> _todosLosSocios = new();
        private List<PagoVista> _todosLosPagos = new();

        public ObservableCollection<PagoVista> Pagos { get; set; } = new();

        public RegistrarPago()
        {
            InitializeComponent();
            CargarSocios();
            CargarPagos();
            cmbMonto.ItemsSource = DatosRecepDemo.ObtenerPreciosMembresias();
            dgPagos.ItemsSource = Pagos;
        }

        private void CargarSocios()
        {
            // TODO integración BD: esto se reemplaza por el repositorio real (SGG.Datos).
            _todosLosSocios = _sociosDemo
                .Select(s => new SocioVista
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    Dni = s.Dni
                })
                .ToList();
        }

        private void CargarPagos()
        {
            _todosLosPagos = DatosRecepDemo.ObtenerPagos()
                .OrderByDescending(p => p.Fecha)
                .Select(p => new PagoVista
                {
                    Socio = p.SocioNombre,
                    Dni = _sociosDemo.FirstOrDefault(s => s.NombreCompleto == p.SocioNombre)?.Dni ?? string.Empty,
                    Monto = $"${p.Monto.ToString("N0", EsAr)}",
                    FechaOriginal = p.Fecha,
                    Fecha = p.Fecha.ToString("dd/MM/yyyy"),
                    Metodo = p.Metodo
                })
                .ToList();

            Pagos = new ObservableCollection<PagoVista>(_todosLosPagos);
        }

        // Búsqueda sin desplegable, mismo patrón que el buscador de Admin (Reportes/GestionSocios):
        // filtra por nombre o DNI y resuelve el socio recién cuando hay UNA única coincidencia.
        private List<SocioVista> BuscarCandidatos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<SocioVista>();

            string criterio = texto.Trim();
            return _todosLosSocios
                .Where(s => s.NombreCompleto.Contains(criterio, StringComparison.OrdinalIgnoreCase)
                         || s.Dni.Contains(criterio, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private void txtBuscarSocio_TextChanged(object sender, TextChangedEventArgs e)
        {
            hintBuscarSocio.Visibility = string.IsNullOrEmpty(txtBuscarSocio.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;

            var candidatos = BuscarCandidatos(txtBuscarSocio.Text);

            if (candidatos.Count == 0)
            {
                bool vacio = string.IsNullOrWhiteSpace(txtBuscarSocio.Text);
                txtEstadoSocio.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x55, 0x55));
                txtEstadoSocio.Text = vacio ? string.Empty : "No se encontró ningún socio.";
                txtEstadoSocio.Visibility = vacio ? Visibility.Collapsed : Visibility.Visible;
                return;
            }

            if (candidatos.Count == 1)
            {
                var socio = candidatos[0];
                txtEstadoSocio.Foreground = new SolidColorBrush(Color.FromRgb(0x5B, 0xE4, 0x9B));
                txtEstadoSocio.Text = $"✔ Socio: {socio.NombreCompleto} · DNI {socio.Dni}";
                txtEstadoSocio.Visibility = Visibility.Visible;
                return;
            }

            txtEstadoSocio.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            txtEstadoSocio.Text = $"{candidatos.Count} coincidencias — seguí escribiendo";
            txtEstadoSocio.Visibility = Visibility.Visible;
        }

        private void AplicarFiltros()
        {
            string texto = txtBuscarPago.Text.Trim();
            DateTime? fecha = dpFiltroFecha.SelectedDate;

            Pagos.Clear();

            var filtrados = _todosLosPagos.AsEnumerable();
            if (fecha.HasValue)
                filtrados = filtrados.Where(p => p.FechaOriginal.Date == fecha.Value.Date);
            if (texto.Length > 0)
                filtrados = filtrados.Where(p =>
                    p.Socio.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    p.Dni.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var pago in filtrados)
                Pagos.Add(pago);
        }

        private void txtBuscarPago_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();

            hintBuscarPago.Visibility = string.IsNullOrEmpty(txtBuscarPago.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void dpFiltroFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => AplicarFiltros();

        private void btnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarPago.Clear();
            dpFiltroFecha.SelectedDate = null;
            AplicarFiltros();
        }

        private void btnRegistrarPago_Click(object sender, RoutedEventArgs e)
        {
            OcultarAvisos();

            // Re-resuelve el socio desde el texto del buscador (nombre o DNI): si no hay
            // exactamente una coincidencia, se mantiene el mismo aviso de siempre.
            var candidatos = BuscarCandidatos(txtBuscarSocio.Text);
            if (candidatos.Count != 1)
            {
                MostrarError("Debe seleccionar un socio.");
                return;
            }
            var socio = candidatos[0];

            if (cmbMonto.SelectedItem is not PrecioMembresiaDemo precio)
            {
                MostrarError("Debe seleccionar un precio de la lista.");
                return;
            }

            if (cmbMetodoPago.SelectedItem is not ComboBoxItem metodoItem ||
                string.IsNullOrWhiteSpace(metodoItem.Content?.ToString()))
            {
                MostrarError("Debe seleccionar un método de pago.");
                return;
            }

            // TODO integración BD: acá se registra el pago real contra SGG.Logica / SGG.Datos (RF-05).
            var nuevo = new PagoVista
            {
                Socio = _sociosDemo.First(s => s.Id == socio.Id).NombreCompleto,
                Dni = socio.Dni,
                Monto = $"${precio.Monto.ToString("N0", EsAr)}",
                FechaOriginal = DateTime.Now,
                Fecha = DateTime.Now.ToString("dd/MM/yyyy"),
                Metodo = metodoItem.Content.ToString() ?? string.Empty
            };

            _todosLosPagos.Insert(0, nuevo);
            AplicarFiltros();

            txtBuscarSocio.Clear();
            cmbMonto.SelectedIndex = -1;
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
        public string Dni { get; set; } = string.Empty;
        public string Monto { get; set; } = string.Empty;
        public DateTime FechaOriginal { get; set; }
        public string Fecha { get; set; } = string.Empty;
        public string Metodo { get; set; } = string.Empty;
    }
}