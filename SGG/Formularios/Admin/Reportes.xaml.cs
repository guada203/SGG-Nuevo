using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using SGG.Controles;
using SGG.Formularios.Recepcionista;

namespace SGG.Formularios.Admin
{
    public partial class Reportes : UserControl
    {
        private string _reporteActual = "pagos";
        private List<ReportePagoVista> _pagosActuales = new();
        private List<ReporteAsistenciaVista> _asistenciasActuales = new();
        private List<ReporteAsistenciaSocioVista> _asistenciasSocios = new();
        private ReporteAsistenciaSocioVista? _asistenciaDetalle; // null = modo lista, no null = modo detalle
        private List<ReporteSocioVista> _sociosActuales = new();
        private List<SocioDemo> _sociosDemo = DatosRecepDemo.ObtenerSocios();
        private ListCollectionView _vistaPagosActual;
        private string _textoBuscarPagos = string.Empty;

        public Reportes()
        {
            InitializeComponent();
            MostrarReportePagos(); // pestaña por defecto al abrir
        }

        // ---------- Manejo de pestañas ----------

        private void tabPagos_Click(object sender, RoutedEventArgs e)
        {
            MarcarPestañaActiva(tabPagos);
            MostrarReportePagos();
        }

        private void tabAsistencias_Click(object sender, RoutedEventArgs e)
        {
            MarcarPestañaActiva(tabAsistencias);
            MostrarReporteAsistencias();
        }

        private void tabSocios_Click(object sender, RoutedEventArgs e)
        {
            MarcarPestañaActiva(tabSocios);
            MostrarReporteSocios();
        }

        private void MarcarPestañaActiva(System.Windows.Controls.Button activa)
        {
            foreach (var boton in new[] { tabPagos, tabAsistencias, tabSocios })
            {
                boton.Background = System.Windows.Media.Brushes.Transparent;
                boton.Foreground = System.Windows.Media.Brushes.Gray;
            }
            activa.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#C5FF00")!;
            activa.Foreground = System.Windows.Media.Brushes.Black;
        }

        // ---------- Datos de ejemplo por reporte (RF-14, RF-15, RF-16) ----------

        private void MostrarReportePagos()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica
            _reporteActual = "pagos";
            _pagosActuales = DatosRecepDemo.ObtenerPagos()
                .Select(p => new ReportePagoVista
                {
                    Socio = p.SocioNombre,
                    Dni = _sociosDemo.FirstOrDefault(s => s.NombreCompleto == p.SocioNombre)?.Dni ?? string.Empty,
                    Monto = p.Monto,
                    Fecha = p.Fecha,
                    Metodo = p.Metodo,
                    Actividad = _sociosDemo.FirstOrDefault(s => s.NombreCompleto == p.SocioNombre)?.TipoMembresia ?? string.Empty
                })
                .ToList();

            dgReporte.Columns.Clear();
            dgReporte.Columns.Add(CrearColumnaTexto("Socio", "Socio"));
            dgReporte.Columns.Add(CrearColumnaTexto("DNI", "Dni"));
            dgReporte.Columns.Add(CrearColumnaTexto("Monto", "Monto", "{0:C0}"));
            dgReporte.Columns.Add(CrearColumnaTexto("Fecha", "Fecha", "{0:dd/MM/yyyy}"));
            dgReporte.Columns.Add(CrearColumnaTexto("Método", "Metodo"));

            // Vista con filtro: la grilla, el resumen, las tortas y las exportaciones
            // reflejan automáticamente el texto escrito en el buscador.
            _vistaPagosActual = new ListCollectionView(_pagosActuales);
            _vistaPagosActual.Filter = FiltrarPagoActual;
            dgReporte.ItemsSource = _vistaPagosActual;

            panelFiltroSocio.Visibility = Visibility.Visible;
            txtResumen.Text = ObtenerTextoResumen();

            // Alimentar tortas de pagos
            MostrarTortasPagos();
        }

        private void MostrarReporteAsistencias()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica
            _reporteActual = "asistencias";

            // Asistencias crudas desde los datos demo (una fila por registro de ingreso).
            _asistenciasActuales = DatosRecepDemo.ObtenerAsistencias()
                .Select(a => new ReporteAsistenciaVista { Socio = a.SocioNombre, FechaHora = a.FechaHora })
                .ToList();

            // Vista por socio: agrupa los registros y calcula total y última visita.
            _asistenciasSocios = _asistenciasActuales
                .GroupBy(a => a.Socio)
                .Select(g => new ReporteAsistenciaSocioVista
                {
                    Socio = g.Key,
                    Total = g.Count(),
                    UltimaVisita = g.Max(a => a.FechaHora)
                })
                .OrderByDescending(s => s.Total)
                .ThenBy(s => s.Socio)
                .ToList();

            MostrarListadoAsistencias();

            // Ocultar tortas y filtro de socio en asistencias
            panelTortas.Visibility = Visibility.Collapsed;
            panelFiltroSocio.Visibility = Visibility.Collapsed;
        }

        // Modo lista: una fila por socio con total de asistencias y última visita.
        private void MostrarListadoAsistencias()
        {
            _asistenciaDetalle = null;

            dgReporte.Columns.Clear();
            dgReporte.Columns.Add(CrearColumnaTexto("Socio", "Socio"));
            dgReporte.Columns.Add(CrearColumnaTexto("Total", "Total", "{0} asistencias"));
            dgReporte.Columns.Add(CrearColumnaTexto("Última visita", "UltimaVisita", "{0:dd/MM/yyyy HH:mm}"));
            dgReporte.Columns.Add(CrearColumnaBotonVerDetalle());

            dgReporte.ItemsSource = _asistenciasSocios;
            txtResumen.Text = ObtenerTextoResumen();
            btnVolverAsistencias.Visibility = Visibility.Collapsed;
        }

        // Modo detalle: historial de asistencias del socio seleccionado (más reciente primero).
        private void MostrarDetalleAsistencias(ReporteAsistenciaSocioVista socio)
        {
            _asistenciaDetalle = socio;

            var historial = _asistenciasActuales
                .Where(a => a.Socio == socio.Socio)
                .OrderByDescending(a => a.FechaHora)
                .ToList();

            dgReporte.Columns.Clear();
            dgReporte.Columns.Add(CrearColumnaTexto("Socio", "Socio"));
            dgReporte.Columns.Add(CrearColumnaTexto("FechaHora", "FechaHora", "{0:dd/MM/yyyy HH:mm}"));

            dgReporte.ItemsSource = historial;
            txtResumen.Text = ObtenerTextoResumen();
            btnVolverAsistencias.Visibility = Visibility.Visible;
        }

        private void BtnVerDetalleAsistencia_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ReporteAsistenciaSocioVista socio)
                MostrarDetalleAsistencias(socio);
        }

        private void btnVolverAsistencias_Click(object sender, RoutedEventArgs e)
        {
            MostrarListadoAsistencias();
        }

        // Columna de acción con el botón "Ver detalle" (creada en código como el resto).
        private DataGridTemplateColumn CrearColumnaBotonVerDetalle()
        {
            var fabricaBoton = new FrameworkElementFactory(typeof(Button));
            fabricaBoton.SetValue(Button.ContentProperty, "Ver detalle");
            fabricaBoton.SetValue(Button.MarginProperty, new Thickness(4, 6, 4, 6));
            fabricaBoton.SetValue(Button.PaddingProperty, new Thickness(12, 4, 12, 4));
            fabricaBoton.SetValue(Button.BackgroundProperty, new SolidColorBrush(Color.FromRgb(0x1A, 0x1A, 0x1A)));
            fabricaBoton.SetValue(Button.ForegroundProperty, Brushes.White);
            fabricaBoton.SetValue(Button.BorderBrushProperty, new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)));
            fabricaBoton.SetValue(Button.CursorProperty, Cursors.Hand);
            fabricaBoton.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnVerDetalleAsistencia_Click));

            var plantilla = new DataTemplate { VisualTree = fabricaBoton };

            return new DataGridTemplateColumn
            {
                Header = "Ver detalle",
                CellTemplate = plantilla,
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
        }

        private void MostrarReporteSocios()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica
            _reporteActual = "socios";

            // Socios desde los datos demo: los mismos nombres que el resto del sistema
            // (GestionSocios, ControlAsistencia, etc.) para que el reporte sea coherente.
            _sociosActuales = DatosRecepDemo.ObtenerSocios()
                .Select(s => new ReporteSocioVista
                {
                    Nombre = s.NombreCompleto,
                    Estado = s.Activo ? "Activo" : "Inactivo",
                    Membresia = s.TipoMembresia,
                    FechaVencimiento = s.FechaVencimiento
                })
                .OrderBy(s => s.Nombre)
                .ToList();

            dgReporte.Columns.Clear();
            dgReporte.Columns.Add(CrearColumnaTexto("Nombre", "Nombre"));
            dgReporte.Columns.Add(CrearColumnaTexto("Estado", "Estado"));
            dgReporte.Columns.Add(CrearColumnaTexto("Membresía", "Membresia"));
            dgReporte.Columns.Add(CrearColumnaTexto("Vencimiento", "FechaVencimiento", "{0:dd/MM/yyyy}"));

            dgReporte.ItemsSource = _sociosActuales;
            txtResumen.Text = ObtenerTextoResumen();

            // Ocultar filtro de socio en socios
            panelFiltroSocio.Visibility = Visibility.Collapsed;

            // Alimentar torta de socios
            MostrarTortasSocios();
        }

        // ---------- Filtro por socio (pestaña PAGOS) ----------

        private bool FiltrarPagoActual(object obj)
        {
            if (string.IsNullOrWhiteSpace(_textoBuscarPagos)) return true;
            var p = (ReportePagoVista)obj;
            return p.Socio.Contains(_textoBuscarPagos, StringComparison.OrdinalIgnoreCase)
                || p.Dni.Contains(_textoBuscarPagos);
        }

        private void txtBuscarPagos_TextChanged(object sender, TextChangedEventArgs e)
        {
            _textoBuscarPagos = txtBuscarPagos.Text?.Trim() ?? string.Empty;
            hintBuscarPagos.Visibility = string.IsNullOrEmpty(txtBuscarPagos.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            AplicarFiltroPagos();
        }

        private void btnLimpiarFiltro_Click(object sender, RoutedEventArgs e)
        {
            txtBuscarPagos.Text = string.Empty;
            _textoBuscarPagos = string.Empty;
            AplicarFiltroPagos();
        }

        private void AplicarFiltroPagos()
        {
            _vistaPagosActual?.Refresh();
            txtResumen.Text = ObtenerTextoResumen();
            MostrarTortasPagos();
        }

        // ---------- Resumen y columnas en código ----------

        private DataGridTextColumn CrearColumnaTexto(string encabezado, string propiedad, string? formato = null)
        {
            return new DataGridTextColumn
            {
                Header = encabezado,
                Binding = new Binding(propiedad) { StringFormat = formato },
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            };
        }

        private string ObtenerTextoResumen()
        {
            switch (_reporteActual)
            {
                case "pagos":
                    if (!string.IsNullOrWhiteSpace(_textoBuscarPagos))
                    {
                        decimal total = PagosVisibles.Sum(p => p.Monto);
                        return $"Resultado: {PagosVisibles.Count} pagos · {total.ToString("C0")}";
                    }
                    decimal totalRecaudado = _pagosActuales.Sum(p => p.Monto);
                    return $"Total recaudado: {totalRecaudado.ToString("C0")}";
                case "asistencias":
                    if (_asistenciaDetalle != null)
                        return $"Historial de {_asistenciaDetalle.Socio}: {_asistenciaDetalle.Total} asistencias";
                    return $"{_asistenciasSocios.Count} socios · {_asistenciasActuales.Count} asistencias totales";
                case "socios":
                    int activos = _sociosActuales.Count(s => s.Estado == "Activo");
                    int inactivos = _sociosActuales.Count - activos;
                    return $"Activos: {activos} · Inactivos: {inactivos}";
                default:
                    return string.Empty;
            }
        }

        private List<ReportePagoVista> PagosVisibles
            => _vistaPagosActual?.Cast<ReportePagoVista>().ToList() ?? _pagosActuales;

        private (string[] Encabezados, List<string[]> Filas) ObtenerDatosExportar()
        {
            switch (_reporteActual)
            {
                case "pagos":
                    var filasPagos = PagosVisibles
                        .Select(p => new[] { p.Socio, p.Dni, p.Monto.ToString("C0"), p.Fecha.ToString("dd/MM/yyyy"), p.Metodo, p.Actividad })
                        .ToList();
                    return (new[] { "Socio", "DNI", "Monto", "Fecha", "Método", "Actividad" }, filasPagos);
                case "asistencias":
                    // Modo detalle: historial del socio seleccionado.
                    if (_asistenciaDetalle != null)
                    {
                        var filasHistorial = _asistenciasActuales
                            .Where(a => a.Socio == _asistenciaDetalle.Socio)
                            .OrderByDescending(a => a.FechaHora)
                            .Select(a => new[] { a.Socio, a.FechaHora.ToString("dd/MM/yyyy HH:mm") })
                            .ToList();
                        return (new[] { "Socio", "FechaHora" }, filasHistorial);
                    }
                    // Modo lista: una fila por socio.
                    var filasAsistencias = _asistenciasSocios
                        .Select(s => new[] { s.Socio, s.Total.ToString(), s.UltimaVisita.ToString("dd/MM/yyyy HH:mm") })
                        .ToList();
                    return (new[] { "Socio", "Total", "Última visita" }, filasAsistencias);
                case "socios":
                    var filasSocios = _sociosActuales
                        .Select(s => new[]
                        {
                            s.Nombre,
                            s.Estado,
                            s.Membresia,
                            s.FechaVencimiento.HasValue ? s.FechaVencimiento.Value.ToString("dd/MM/yyyy") : string.Empty
                        })
                        .ToList();
                    return (new[] { "Nombre", "Estado", "Membresía", "Vencimiento" }, filasSocios);
                default:
                    return (Array.Empty<string>(), new List<string[]>());
            }
        }

        // ---------- Tortas (gráficos por pestaña) ----------

        private void MostrarTortasPagos()
        {
            panelTortas.Visibility = Visibility.Visible;
            tortasSocios.Visibility = Visibility.Collapsed;
            tortasPagos.Visibility = Visibility.Visible;

            // Paleta fija por actividad
            var coloresActividad = new Dictionary<string, string>
            {
                { "Musculación", "#C5FF00" },
                { "Funcional", "#3B82F6" },
                { "Combinado", "#A855F7" }
            };

            tortaActividad.SetDatos(
                PagosVisibles
                    .GroupBy(p => p.Actividad)
                    .Select(g => new GraficoTortaItem
                    {
                        Etiqueta = g.Key,
                        Valor = (double)g.Sum(p => p.Monto),
                        ColorHex = coloresActividad.TryGetValue(g.Key, out var c) ? c : "#888888"
                    })
                    .OrderByDescending(i => i.Valor)
                    .ToList()
            );

            // Paleta fija por método de pago
            var coloresMetodo = new Dictionary<string, string>
            {
                { "Efectivo", "#10B981" },
                { "Tarjeta", "#3B82F6" },
                { "Transferencia", "#F59E0B" }
            };

            tortaMetodo.SetDatos(
                PagosVisibles
                    .GroupBy(p => p.Metodo)
                    .Select(g => new GraficoTortaItem
                    {
                        Etiqueta = g.Key,
                        Valor = (double)g.Sum(p => p.Monto),
                        ColorHex = coloresMetodo.TryGetValue(g.Key, out var c) ? c : "#888888"
                    })
                    .OrderByDescending(i => i.Valor)
                    .ToList()
            );
        }

        private void MostrarTortasSocios()
        {
            panelTortas.Visibility = Visibility.Visible;
            tortasPagos.Visibility = Visibility.Collapsed;
            tortasSocios.Visibility = Visibility.Visible;

            int activos = _sociosActuales.Count(s => s.Estado == "Activo");
            int inactivos = _sociosActuales.Count - activos;

            tortaSocios.SetDatos(new List<GraficoTortaItem>
            {
                new() { Etiqueta = "Activos", Valor = activos, ColorHex = "#C5FF00" },
                new() { Etiqueta = "Inactivos", Valor = inactivos, ColorHex = "#FF5555" }
            });
        }

        // ---------- Exportar (RF-17) ----------

        // Exporta el reporte actual a CSV (separador ";" para Excel con configuración regional
        // argentina) con BOM UTF-8 para que los acentos se vean bien en Excel.
        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cultura = new CultureInfo("es-AR");
                string mesNombre = DateTime.Now.ToString("MMMMyyyy", cultura).ToLowerInvariant();

                var dialogo = new SaveFileDialog
                {
                    Title = "Exportar reporte",
                    Filter = "CSV (*.csv)|*.csv",
                    FileName = $"reporte_{_reporteActual}_{mesNombre}.csv"
                };

                if (dialogo.ShowDialog() != true)
                    return;

                var datos = ObtenerDatosExportar();
                var contenido = new StringBuilder();
                contenido.AppendLine(string.Join(";", datos.Encabezados));
                foreach (var fila in datos.Filas)
                    contenido.AppendLine(string.Join(";", fila));

                File.WriteAllText(dialogo.FileName, contenido.ToString(), new UTF8Encoding(true));

                MessageBox.Show(
                    $"Reporte exportado correctamente:\n{dialogo.FileName}",
                    "Exportar Excel",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo exportar el reporte: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Genera un FlowDocument y lo envía al diálogo de impresión para que el usuario
        // pueda elegir "Microsoft Print to PDF" (PDF real sin librerías externas).
        private void btnExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialogo = new PrintDialog();
                if (dialogo.ShowDialog() != true)
                    return;

                var documento = ConstruirFlowDocument();
                dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator, "Reporte SGG");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo generar el PDF: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FlowDocument ConstruirFlowDocument()
        {
            string titulo;
            if (_reporteActual == "pagos" && !string.IsNullOrWhiteSpace(_textoBuscarPagos))
                titulo = $"Reporte de Pagos — filtrado por \"{_textoBuscarPagos}\"";
            else if (_reporteActual == "asistencias" && _asistenciaDetalle != null)
                titulo = $"Reporte de Asistencias — Historial de {_asistenciaDetalle.Socio}";
            else
                titulo = _reporteActual switch
                {
                    "pagos" => "Reporte de Pagos",
                    "asistencias" => "Reporte de Asistencias",
                    _ => "Reporte de Socios"
                };

            var documento = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new FontFamily("Segoe UI")
            };

            documento.Blocks.Add(new Paragraph(new Run(titulo)) { FontSize = 20, FontWeight = FontWeights.Bold });
            documento.Blocks.Add(new Paragraph(new Run($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")) { FontSize = 11, Foreground = Brushes.Gray });
            documento.Blocks.Add(new Paragraph(new Run(ObtenerTextoResumen())) { FontSize = 12, Foreground = Brushes.Gray, Margin = new Thickness(0, 0, 0, 12) });

            var datos = ObtenerDatosExportar();

            var tabla = new Table { CellSpacing = 0 };
            for (int i = 0; i < datos.Encabezados.Length; i++)
                tabla.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            var grupo = new TableRowGroup();

            var filaEncabezados = new TableRow();
            foreach (var encabezado in datos.Encabezados)
            {
                var celda = new TableCell(new Paragraph(new Run(encabezado)) { FontWeight = FontWeights.Bold });
                celda.BorderBrush = Brushes.Gray;
                celda.BorderThickness = new Thickness(0.5);
                celda.Padding = new Thickness(6);
                filaEncabezados.Cells.Add(celda);
            }
            grupo.Rows.Add(filaEncabezados);

            foreach (var fila in datos.Filas)
            {
                var filaTabla = new TableRow();
                foreach (var valor in fila)
                {
                    var celda = new TableCell(new Paragraph(new Run(valor)));
                    celda.BorderBrush = Brushes.Gray;
                    celda.BorderThickness = new Thickness(0.5);
                    celda.Padding = new Thickness(6);
                    filaTabla.Cells.Add(celda);
                }
                grupo.Rows.Add(filaTabla);
            }

            tabla.RowGroups.Add(grupo);
            documento.Blocks.Add(tabla);
            return documento;
        }
    }

    // ---------- Clases auxiliares para mostrar cada reporte ----------

    public class ReportePagoVista
    {
        public string Socio { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Metodo { get; set; } = string.Empty;
        public string Actividad { get; set; } = string.Empty;
    }

    public class ReporteAsistenciaVista
    {
        public string Socio { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
    }

    public class ReporteAsistenciaSocioVista
    {
        public string Socio { get; set; } = string.Empty;
        public int Total { get; set; }
        public DateTime UltimaVisita { get; set; }
    }

    public class ReporteSocioVista
    {
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Membresia { get; set; } = string.Empty;
        public DateTime? FechaVencimiento { get; set; }
    }
}