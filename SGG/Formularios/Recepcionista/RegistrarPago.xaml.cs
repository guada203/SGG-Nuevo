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
using SGG.Dominio.Entidades;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Recepcionista
{
    public partial class RegistrarPago : UserControl
    {
        private static readonly CultureInfo EsAr = new("es-AR");

        private List<SocioVista> _todosLosSocios = new();
        private List<PagoVista> _todosLosPagos = new();
        private ICollectionView _vistaSocios;
        private string _textoBusquedaSocio = "";

        // Socios reales (con Membresia cargada) indexados por Id: sirven para resolver el
        // precio de la membresía y para completar nombre/DNI en las filas del historial.
        private readonly Dictionary<int, Socio> _sociosPorId = new();

        // Precio de la membresía del socio elegido. null = sin selección o sin membresía.
        private decimal? _montoMembresia;

        public ObservableCollection<PagoVista> Pagos { get; set; } = new();

        public RegistrarPago()
        {
            InitializeComponent();
            CargarSocios();
            CargarPagos();
            dgPagos.ItemsSource = Pagos;
        }

        private void CargarSocios()
        {
            var socios = new ServicioSocios().ObtenerTodos();

            _sociosPorId.Clear();
            foreach (var socio in socios)
                _sociosPorId[socio.Id] = socio;

            _todosLosSocios = socios
                .Select(s => new SocioVista
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    Dni = s.Dni
                })
                .OrderBy(s => s.NombreCompleto)
                .ToList();

            // El diccionario debe estar listo ANTES de asignar ItemsSource: el combo puede
            // disparar SelectionChanged y eso consulta el precio de la membresía.
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
            _todosLosPagos = new ServicioPagos().ObtenerTodos()
                .OrderByDescending(p => p.FechaPago)
                .Select(MapearPago)
                .ToList();

            Pagos = new ObservableCollection<PagoVista>(_todosLosPagos);
        }

        // Proyecta la entidad Pago a la fila que muestra la grilla del historial.
        private static PagoVista MapearPago(Pago pago) => new()
        {
            Socio = pago.Socio != null ? $"{pago.Socio.Nombre} {pago.Socio.Apellido}".Trim() : "—",
            Dni = pago.Socio?.Dni ?? string.Empty,
            Monto = $"${pago.Monto.ToString("N0", EsAr)}",
            FechaOriginal = pago.FechaPago,
            Fecha = pago.FechaPago.ToString("dd/MM/yyyy"),
            Metodo = pago.MetodoPago
        };

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

        // El precio no se elige más: se toma de la membresía real del socio seleccionado.
        private void cmbSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarPrecioMembresia();
        }

        private void ActualizarPrecioMembresia()
        {
            _montoMembresia = null;

            if (cmbSocios.SelectedItem is not SocioVista vista)
            {
                txtPrecio.Text = string.Empty;
                return;
            }

            if (!_sociosPorId.TryGetValue(vista.Id, out var socio) || socio.Membresia == null)
            {
                txtPrecio.Text = "Sin membresía";
                return;
            }

            if (socio.Membresia.Precio <= 0)
            {
                txtPrecio.Text = "Sin precio asignado";
                return;
            }

            _montoMembresia = socio.Membresia.Precio;
            txtPrecio.Text = $"${socio.Membresia.Precio.ToString("N0", EsAr)}";
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

            if (_montoMembresia == null || _montoMembresia.Value <= 0)
            {
                MostrarError("El socio seleccionado no tiene una membresía con un precio válido para cobrar.");
                return;
            }

            if (cmbMetodoPago.SelectedItem is not ComboBoxItem metodoItem ||
                string.IsNullOrWhiteSpace(metodoItem.Content?.ToString()))
            {
                MostrarError("Debe seleccionar un método de pago.");
                return;
            }

            var servicioPagos = new ServicioPagos();
            var (exitoso, mensaje, pagoGuardado) = servicioPagos.RegistrarPago(
                socio.Id, _montoMembresia.Value, metodoItem.Content.ToString() ?? string.Empty);

            if (!exitoso || pagoGuardado == null)
            {
                MostrarError(mensaje);
                return;
            }

            // El pago recién insertado vuelve sin la navegación a Socio (el repositorio
            // solo agrega el pago), así que se la asociamos desde el socio ya cargado
            // para que la fila del historial muestre nombre y DNI y no "—".
            pagoGuardado.Socio = _sociosPorId.GetValueOrDefault(socio.Id);

            // El comprobante se emite con el pago ya guardado en la base.
            var concepto = ConstruirConcepto(socio.Id);

            _todosLosPagos.Insert(0, MapearPago(pagoGuardado));
            AplicarFiltros();

            LimpiarFormulario();

            new VentanaComprobante(pagoGuardado, socio.NombreCompleto, socio.Dni, concepto).ShowDialog();

            txtExito.Text = mensaje;
            txtExito.Visibility = Visibility.Visible;
        }

        private string ConstruirConcepto(int socioId)
        {
            if (!_sociosPorId.TryGetValue(socioId, out var socio) || socio.Membresia == null)
                return "Cuota membresía";

            return $"Cuota membresía — {EtiquetaActividad(socio.Membresia.TipoActividad)}";
        }

        // Mismo rótulo que usa el combo de actividad de AltaMembresia.xaml.
        private static string EtiquetaActividad(TipoActividad tipo) => tipo switch
        {
            TipoActividad.Musculacion => "Musculación",
            TipoActividad.Funcional => "Funcional",
            TipoActividad.Combinado => "Combinado",
            _ => tipo.ToString()
        };

        private void LimpiarFormulario()
        {
            cmbSocios.SelectedIndex = -1;
            cmbSocios.Text = string.Empty;
            cmbMetodoPago.SelectedIndex = -1;
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
