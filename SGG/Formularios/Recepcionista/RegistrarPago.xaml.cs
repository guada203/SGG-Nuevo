using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace SGG.Formularios.Recepcionista
{
    public partial class RegistrarPago : UserControl
    {
        private static readonly CultureInfo EsAr = new("es-AR");

        private readonly List<SocioDemo> _sociosDemo = DatosRecepDemo.ObtenerSocios();
        private List<SocioVista> _todosLosSocios = new();
        private List<PagoVista> _todosLosPagos = new();
        private ICollectionView _vistaSocios;
        private string _textoBusquedaSocio = "";

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

            cmbSocios.ItemsSource = _todosLosSocios;

            // La "vista" es una capa sobre la lista que permite ocultar elementos
            // sin sacarlos de la lista original.
            _vistaSocios = CollectionViewSource.GetDefaultView(cmbSocios.ItemsSource);
            _vistaSocios.Filter = FiltrarSocio;

            // Escuchamos lo que se escribe dentro del propio ComboBox
            cmbSocios.AddHandler(TextBoxBase.TextChangedEvent,
                                 new TextChangedEventHandler(cmbSocios_TextChanged));
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

        private bool FiltrarSocio(object item)
        {
            if (string.IsNullOrEmpty(_textoBusquedaSocio)) return true;

            var socio = item as SocioVista;
            if (socio == null) return false;

            return socio.NombreCompleto.ToLower().Contains(_textoBusquedaSocio)
                || socio.Dni.ToLower().Contains(_textoBusquedaSocio);
        }

        private void cmbSocios_TextChanged(object sender, TextChangedEventArgs e)
        {
            var seleccionado = cmbSocios.SelectedItem as SocioVista;

            // Si el texto coincide exactamente con el socio ya elegido, es porque
            // alguien lo seleccionó: limpiamos el filtro y no abrimos nada.
            if (seleccionado != null && cmbSocios.Text == seleccionado.NombreConDni)
            {
                _textoBusquedaSocio = "";
                _vistaSocios?.Refresh();
                return;
            }

            _textoBusquedaSocio = cmbSocios.Text?.Trim().ToLower() ?? "";
            _vistaSocios?.Refresh();

            if (!cmbSocios.IsDropDownOpen)
            {
                cmbSocios.IsDropDownOpen = true;
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

            if (cmbSocios.SelectedItem is not SocioVista socio)
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

            cmbSocios.SelectedIndex = -1;
            cmbSocios.Text = string.Empty;
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