using System;
using System.Collections.Generic;
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

        private readonly List<SocioDemo> _sociosDemo = DatosRecepDemo.ObtenerSocios();
        private List<SocioVista> _todosLosSocios = new();
        private List<PagoVista> _todosLosPagos = new();
        private bool _restaurandoSocios;

        public ObservableCollection<SocioVista> Socios { get; set; } = new();
        public ObservableCollection<PagoVista> Pagos { get; set; } = new();

        public RegistrarPago()
        {
            InitializeComponent();
            // ComboBox no expone TextChanged en XAML; el TextBox editable interno lo
            // dispara como evento enrutado que burbujea hasta el ComboBox.
            cmbSocio.AddHandler(TextBox.TextChangedEvent, new TextChangedEventHandler(cmbSocio_TextChanged));
            CargarSocios();
            CargarPagos();
            cmbSocio.ItemsSource = Socios;
            cmbMonto.ItemsSource = DatosRecepDemo.ObtenerPreciosMembresias();
            dgPagos.ItemsSource = Pagos;
        }

        private void CargarSocios()
        {
            _todosLosSocios = _sociosDemo
                .Select(s => new SocioVista
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido} — DNI {s.Dni}".Trim(),
                    Dni = s.Dni
                })
                .ToList();

            Socios.Clear();
            foreach (var socio in _todosLosSocios)
                Socios.Add(socio);
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

        private void RestaurarListaSocios()
        {
            if (!ReferenceEquals(cmbSocio.ItemsSource, Socios))
                cmbSocio.ItemsSource = Socios;
        }

        private void cmbSocio_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_restaurandoSocios) return;
            if (cmbSocio.SelectedItem is SocioVista sel && sel.NombreCompleto == cmbSocio.Text) return;

            string texto = cmbSocio.Text;
            var filtrados = _todosLosSocios
                .Where(s => s.NombreCompleto.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            cmbSocio.ItemsSource = filtrados;
            cmbSocio.IsDropDownOpen = filtrados.Count > 0 && texto.Length > 0;
        }

        private void cmbSocio_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSocio.SelectedItem is SocioVista)
            {
                _restaurandoSocios = true;
                RestaurarListaSocios();
                _restaurandoSocios = false;
            }
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

        private void txtBuscarPago_TextChanged(object sender, TextChangedEventArgs e) => AplicarFiltros();

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

            if (cmbSocio.SelectedItem is not SocioVista socio)
            {
                MostrarError("Debe seleccionar un socio.");
                return;
            }

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

            cmbSocio.SelectedItem = null;
            cmbSocio.Text = "";
            RestaurarListaSocios();
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