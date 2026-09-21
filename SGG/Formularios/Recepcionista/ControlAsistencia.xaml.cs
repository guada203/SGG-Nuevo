using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SGG.Formularios.Recepcionista
{
    public partial class ControlAsistencia : UserControl
    {
        public ObservableCollection<SocioVista> Socios { get; set; } = new();
        public ObservableCollection<AsistenciaVista> AsistenciasHoy { get; set; } = new();
        public ObservableCollection<AsistenciaHistorialVista> HistorialSocio { get; set; } = new();

        public ControlAsistencia()
        {
            InitializeComponent();
            CargarSocios();
            CargarAsistenciasDeHoy();
            dgAsistenciasHoy.ItemsSource = AsistenciasHoy;
            dgHistorialSocio.ItemsSource = HistorialSocio;
        }

        // Búsqueda sin desplegable, mismo patrón que el buscador de Admin (Reportes/GestionSocios):
        // filtra por nombre o DNI y resuelve el socio recién cuando hay UNA única coincidencia.
        private List<SocioVista> BuscarCandidatos(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new List<SocioVista>();

            string criterio = texto.Trim();
            return Socios
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
                txtEstadoSocioIngreso.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x55, 0x55));
                txtEstadoSocioIngreso.Text = vacio ? string.Empty : "No se encontró ningún socio.";
                txtEstadoSocioIngreso.Visibility = vacio ? Visibility.Collapsed : Visibility.Visible;
                return;
            }

            if (candidatos.Count == 1)
            {
                var socio = candidatos[0];
                txtEstadoSocioIngreso.Foreground = new SolidColorBrush(Color.FromRgb(0x5B, 0xE4, 0x9B));
                txtEstadoSocioIngreso.Text = $"✔ Socio: {socio.NombreCompleto} · DNI {socio.Dni}";
                txtEstadoSocioIngreso.Visibility = Visibility.Visible;
                return;
            }

            txtEstadoSocioIngreso.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
            txtEstadoSocioIngreso.Text = $"{candidatos.Count} coincidencias — seguí escribiendo";
            txtEstadoSocioIngreso.Visibility = Visibility.Visible;
        }

        private void txtBuscarSocioHistorial_TextChanged(object sender, TextChangedEventArgs e)
        {
            hintBuscarSocioHistorial.Visibility = string.IsNullOrEmpty(txtBuscarSocioHistorial.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
            ActualizarHistorial();
        }

        // RF-10: historial de asistencias del socio resuelto por el buscador
        private void ActualizarHistorial()
        {
            HistorialSocio.Clear();
            txtSinHistorial.Visibility = Visibility.Collapsed;

            var candidatos = BuscarCandidatos(txtBuscarSocioHistorial.Text);

            if (candidatos.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(txtBuscarSocioHistorial.Text))
                {
                    txtSinHistorial.Text = "No se encontró ningún socio.";
                    txtSinHistorial.Visibility = Visibility.Visible;
                }
                return;
            }

            if (candidatos.Count > 1)
            {
                txtSinHistorial.Text = $"{candidatos.Count} coincidencias — seguí escribiendo";
                txtSinHistorial.Visibility = Visibility.Visible;
                return;
            }

            var socio = candidatos[0];
            var registros = DatosRecepDemo.ObtenerAsistencias()
                .Where(a => a.SocioNombre == socio.NombreCompleto)
                .OrderByDescending(a => a.FechaHora)
                .Select(a => new AsistenciaHistorialVista
                {
                    Fecha = a.FechaHora.ToString("dd/MM/yyyy"),
                    Hora = a.FechaHora.ToString("HH:mm")
                });

            foreach (var r in registros)
                HistorialSocio.Add(r);

            if (HistorialSocio.Count == 0)
            {
                txtSinHistorial.Text = $"Sin asistencias registradas para {socio.NombreCompleto}.";
                txtSinHistorial.Visibility = Visibility.Visible;
            }
        }

        private void CargarSocios()
        {
            foreach (var s in DatosRecepDemo.ObtenerSocios())
            {
                Socios.Add(new SocioVista
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    Dni = s.Dni,
                    EstadoCuota = DatosRecepDemo.EstadoCuota(s)
                });
            }
        }

        private void CargarAsistenciasDeHoy()
        {
            var deHoy = DatosRecepDemo.ObtenerAsistencias()
                .Where(a => a.FechaHora.Date == DateTime.Today)
                .OrderBy(a => a.FechaHora)
                .Select(a => new AsistenciaVista
                {
                    Socio = a.SocioNombre,
                    Hora = a.FechaHora.ToString("HH:mm"),
                    EstadoCuota = EstadoCuotaDe(a.SocioNombre)
                });

            foreach (var a in deHoy)
                AsistenciasHoy.Add(a);
        }

        private void btnRegistrarIngreso_Click(object sender, RoutedEventArgs e)
        {
            OcultarMensaje();

            // Re-resuelve el socio desde el texto del buscador (nombre o DNI).
            var candidatos = BuscarCandidatos(txtBuscarSocio.Text);
            if (candidatos.Count != 1)
            {
                MostrarMensaje("Debe seleccionar un socio.", esError: true);
                return;
            }
            var socio = candidatos[0];

            var demo = DatosRecepDemo.Socios.FirstOrDefault(s => s.Id == socio.Id);
            if (demo == null)
            {
                MostrarMensaje("El socio no existe en el padrón.", esError: true);
                return;
            }

            // RF-09: validación real de cuota vencida antes de permitir el ingreso
            if (demo.FechaVencimiento < DateTime.Today)
            {
                MostrarMensaje(
                    $"El socio tiene la cuota vencida (vence: {demo.FechaVencimiento.ToString("dd/MM/yyyy")}). No puede ingresar.",
                    esError: true);
                return;
            }

            // RF-08: registrar el ingreso con la hora actual
            AsistenciasHoy.Add(new AsistenciaVista
            {
                Socio = socio.NombreCompleto,
                Hora = DateTime.Now.ToString("HH:mm"),
                EstadoCuota = "Al día"
            });

            MostrarMensaje($"✔ Ingreso registrado correctamente para {socio.NombreCompleto}.", esError: false);
            txtBuscarSocio.Clear();
        }

        private static string EstadoCuotaDe(string nombreCompleto)
        {
            var demo = DatosRecepDemo.Socios.FirstOrDefault(s => s.NombreCompleto == nombreCompleto);
            return demo != null ? DatosRecepDemo.EstadoCuota(demo) : "Al día";
        }

        private void MostrarMensaje(string texto, bool esError)
        {
            txtMensaje.Text = texto;
            txtMensaje.Foreground = esError
                ? new SolidColorBrush(Color.FromRgb(0xFF, 0x55, 0x55))
                : new SolidColorBrush(Color.FromRgb(0x5B, 0xE4, 0x9B));
            txtMensaje.Visibility = Visibility.Visible;
        }

        private void OcultarMensaje()
        {
            txtMensaje.Visibility = Visibility.Collapsed;
        }
    }

    public class AsistenciaVista
    {
        public string Socio { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
        public string EstadoCuota { get; set; } = string.Empty;
    }

    public class AsistenciaHistorialVista
    {
        public string Fecha { get; set; } = string.Empty;
        public string Hora { get; set; } = string.Empty;
    }
}