using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Win32;
using SGG.Controles;

namespace SGG.Formularios.Admin
{
    public partial class Reportes : UserControl
    {
        private string _reporteActual = "pagos";
        private List<ReportePagoVista> _pagosActuales = new();
        private List<ReporteAsistenciaVista> _asistenciasActuales = new();
        private List<ReporteSocioVista> _sociosActuales = new();

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
            _pagosActuales = new List<ReportePagoVista>
            {
                new() { Socio = "Carolina Méndez", Monto = 18000, Fecha = new DateTime(2026,8,6), Metodo = "Efectivo", Actividad = "Musculación" },
                new() { Socio = "Tomás Restrepo", Monto = 15000, Fecha = new DateTime(2026,8,8), Metodo = "Tarjeta", Actividad = "Funcional" },
                new() { Socio = "Lucía Vargas", Monto = 22000, Fecha = new DateTime(2026,8,12), Metodo = "Transferencia", Actividad = "Combinado" },
                new() { Socio = "Martín Aguirre", Monto = 12000, Fecha = new DateTime(2026,8,15), Metodo = "Efectivo", Actividad = "Musculación" },
                new() { Socio = "Sofía Fernández", Monto = 30000, Fecha = new DateTime(2026,8,19), Metodo = "Tarjeta", Actividad = "Funcional" },
                new() { Socio = "Diego Sosa", Monto = 16000, Fecha = new DateTime(2026,8,22), Metodo = "Efectivo", Actividad = "Combinado" },
                new() { Socio = "Valentina Ríos", Monto = 35000, Fecha = new DateTime(2026,8,27), Metodo = "Transferencia", Actividad = "Funcional" },
                new() { Socio = "Joaquín Pereyra", Monto = 14000, Fecha = new DateTime(2026,8,29), Metodo = "Tarjeta", Actividad = "Musculación" },
                new() { Socio = "Agustina Cabral", Monto = 19000, Fecha = new DateTime(2026,9,1), Metodo = "Efectivo", Actividad = "Combinado" },
                new() { Socio = "Nicolás Duarte", Monto = 25000, Fecha = new DateTime(2026,9,2), Metodo = "Transferencia", Actividad = "Musculación" },
                new() { Socio = "Florencia Gómez", Monto = 17500, Fecha = new DateTime(2026,9,4), Metodo = "Tarjeta", Actividad = "Funcional" },
                new() { Socio = "Ramiro Benítez", Monto = 13000, Fecha = new DateTime(2026,9,5), Metodo = "Efectivo", Actividad = "Combinado" },
            };

            dgReporte.Columns.Clear();
            dgReporte.Columns.Add(CrearColumnaTexto("Socio", "Socio"));
            dgReporte.Columns.Add(CrearColumnaTexto("Monto", "Monto", "{0:C0}"));
            dgReporte.Columns.Add(CrearColumnaTexto("Fecha", "Fecha", "{0:dd/MM/yyyy}"));
            dgReporte.Columns.Add(CrearColumnaTexto("Método", "Metodo"));

            dgReporte.ItemsSource = _pagosActuales;
            txtResumen.Text = ObtenerTextoResumen();

            // Alimentar tortas de pagos
            MostrarTortasPagos();
        }

        private void MostrarReporteAsistencias()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica
            _reporteActual = "asistencias";
            _asistenciasActuales = new List<ReporteAsistenciaVista>
            {
                new() { Socio = "Carolina Méndez", FechaHora = new DateTime(2026,8,21,8,42,0) },
                new() { Socio = "Tomás Restrepo", FechaHora = new DateTime(2026,8,21,18,10,0) },
                new() { Socio = "Lucía Vargas", FechaHora = new DateTime(2026,8,22,9,15,0) },
                new() { Socio = "Martín Aguirre", FechaHora = new DateTime(2026,8,24,17,30,0) },
                new() { Socio = "Sofía Fernández", FechaHora = new DateTime(2026,8,26,8,5,0) },
                new() { Socio = "Diego Sosa", FechaHora = new DateTime(2026,8,28,19,45,0) },
                new() { Socio = "Valentina Ríos", FechaHora = new DateTime(2026,8,29,10,20,0) },
                new() { Socio = "Joaquín Pereyra", FechaHora = new DateTime(2026,9,1,8,55,0) },
                new() { Socio = "Agustina Cabral", FechaHora = new DateTime(2026,9,2,18,33,0) },
                new() { Socio = "Nicolás Duarte", FechaHora = new DateTime(2026,9,3,9,2,0) },
                new() { Socio = "Florencia Gómez", FechaHora = new DateTime(2026,9,4,17,12,0) },
                new() { Socio = "Ramiro Benítez", FechaHora = new DateTime(2026,9,5,8,30,0) },
            };

            dgReporte.Columns.Clear();
            dgReporte.Columns.Add(CrearColumnaTexto("Socio", "Socio"));
            dgReporte.Columns.Add(CrearColumnaTexto("FechaHora", "FechaHora", "{0:dd/MM/yyyy HH:mm}"));

            dgReporte.ItemsSource = _asistenciasActuales;
            txtResumen.Text = ObtenerTextoResumen();

            // Ocultar tortas en asistencias
            panelTortas.Visibility = Visibility.Collapsed;
        }

        private void MostrarReporteSocios()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica
            _reporteActual = "socios";
            _sociosActuales = new List<ReporteSocioVista>
            {
                new() { Nombre = "Carolina Méndez", Estado = "Activo", Membresia = "Musculación", FechaVencimiento = new DateTime(2026,9,30) },
                new() { Nombre = "Tomás Restrepo", Estado = "Activo", Membresia = "Funcional", FechaVencimiento = new DateTime(2026,10,15) },
                new() { Nombre = "Lucía Vargas", Estado = "Inactivo", Membresia = "Funcional", FechaVencimiento = new DateTime(2026,8,12) },
                new() { Nombre = "Martín Aguirre", Estado = "Activo", Membresia = "Combinado", FechaVencimiento = new DateTime(2026,9,20) },
                new() { Nombre = "Sofía Fernández", Estado = "Activo", Membresia = "Musculación", FechaVencimiento = new DateTime(2026,10,25) },
                new() { Nombre = "Diego Sosa", Estado = "Inactivo", Membresia = "Combinado", FechaVencimiento = new DateTime(2026,8,10) },
                new() { Nombre = "Valentina Ríos", Estado = "Activo", Membresia = "Funcional", FechaVencimiento = new DateTime(2026,10,5) },
                new() { Nombre = "Joaquín Pereyra", Estado = "Activo", Membresia = "Musculación", FechaVencimiento = new DateTime(2026,9,1) },
                new() { Nombre = "Agustina Cabral", Estado = "Inactivo", Membresia = "Musculación", FechaVencimiento = new DateTime(2026,8,28) },
                new() { Nombre = "Nicolás Duarte", Estado = "Activo", Membresia = "Combinado", FechaVencimiento = new DateTime(2026,9,12) },
                new() { Nombre = "Florencia Gómez", Estado = "Activo", Membresia = "Funcional", FechaVencimiento = new DateTime(2026,9,18) },
                new() { Nombre = "Ramiro Benítez", Estado = "Inactivo", Membresia = "Musculación", FechaVencimiento = new DateTime(2026,9,5) },
            };

            dgReporte.Columns.Clear();
            dgReporte.Columns.Add(CrearColumnaTexto("Nombre", "Nombre"));
            dgReporte.Columns.Add(CrearColumnaTexto("Estado", "Estado"));
            dgReporte.Columns.Add(CrearColumnaTexto("Membresía", "Membresia"));
            dgReporte.Columns.Add(CrearColumnaTexto("Vencimiento", "FechaVencimiento", "{0:dd/MM/yyyy}"));

            dgReporte.ItemsSource = _sociosActuales;
            txtResumen.Text = ObtenerTextoResumen();

            // Alimentar torta de socios
            MostrarTortasSocios();
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
                    decimal total = _pagosActuales.Sum(p => p.Monto);
                    return $"Total recaudado: {total.ToString("C0")}";
                case "asistencias":
                    return $"Total de asistencias: {_asistenciasActuales.Count}";
                case "socios":
                    int activos = _sociosActuales.Count(s => s.Estado == "Activo");
                    int inactivos = _sociosActuales.Count - activos;
                    return $"Activos: {activos} · Inactivos: {inactivos}";
                default:
                    return string.Empty;
            }
        }

        private (string[] Encabezados, List<string[]> Filas) ObtenerDatosExportar()
        {
            switch (_reporteActual)
            {
                case "pagos":
                    var filasPagos = _pagosActuales
                        .Select(p => new[] { p.Socio, p.Monto.ToString("C0"), p.Fecha.ToString("dd/MM/yyyy"), p.Metodo })
                        .ToList();
                    return (new[] { "Socio", "Monto", "Fecha", "Método" }, filasPagos);
                case "asistencias":
                    var filasAsistencias = _asistenciasActuales
                        .Select(a => new[] { a.Socio, a.FechaHora.ToString("dd/MM/yyyy HH:mm") })
                        .ToList();
                    return (new[] { "Socio", "FechaHora" }, filasAsistencias);
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
                _pagosActuales
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
                _pagosActuales
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
            string titulo = _reporteActual switch
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

    public class ReporteSocioVista
    {
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Membresia { get; set; } = string.Empty;
        public DateTime? FechaVencimiento { get; set; }
    }
}