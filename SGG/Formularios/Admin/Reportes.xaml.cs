using SGG.Formularios.Login;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SGG.Formularios.Admin
{
    public partial class Reportes : Window
    {
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
            var pagos = new List<ReportePagoVista>
            {
                new() { Socio = "Carolina Méndez", Monto = 18000, Fecha = new DateTime(2026,8,1), Metodo = "Efectivo" },
                new() { Socio = "Tomás Restrepo", Monto = 15000, Fecha = new DateTime(2026,8,3), Metodo = "Tarjeta" },
            };
            dgReporte.ItemsSource = pagos;
        }

        private void MostrarReporteAsistencias()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica
            var asistencias = new List<ReporteAsistenciaVista>
            {
                new() { Socio = "Carolina Méndez", FechaHora = new DateTime(2026,8,21,8,42,0) },
                new() { Socio = "Tomás Restrepo", FechaHora = new DateTime(2026,8,21,18,10,0) },
            };
            dgReporte.ItemsSource = asistencias;
        }

        private void MostrarReporteSocios()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica
            var socios = new List<ReporteSocioVista>
            {
                new() { Nombre = "Carolina Méndez", Estado = "Activo", Membresia = "Musculación" },
                new() { Nombre = "Lucía Vargas", Estado = "Inactivo", Membresia = "Funcional" },
            };
            dgReporte.ItemsSource = socios;
        }

        // ---------- Exportar (RF-17) ----------

        private void btnExportarPdf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Acá se va a generar el PDF del reporte actual.");
        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Acá se va a generar el Excel del reporte actual.");
        }

        // ---------- Navegación ----------

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            var dashboard = new VentanaPrincipalAdmin();
            dashboard.Show();
            this.Close();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            var gestionUsuarios = new GestionUsuarios();
            gestionUsuarios.Show();
            this.Close();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var ventanaRol = new VentanaSeleccionRol();
            ventanaRol.Show();
            this.Close();
        }
    }

    // ---------- Clases auxiliares para mostrar cada reporte ----------

    public class ReportePagoVista
    {
        public string Socio { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Metodo { get; set; } = string.Empty;
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
    }
}
