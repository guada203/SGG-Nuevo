using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace SGG.Formularios.Recepcionista
{
    public partial class ControlAsistencia : UserControl
    {
        public ObservableCollection<SocioVista> Socios { get; set; } = new();
        public ObservableCollection<AsistenciaVista> AsistenciasHoy { get; set; } = new();
        public ObservableCollection<AsistenciaHistorialVista> HistorialSocio { get; set; } = new();

        private ICollectionView _vistaSocioIngreso;
        private string _textoBusquedaSocioIngreso = "";
        private ICollectionView _vistaSocioHistorial;
        private string _textoBusquedaSocioHistorial = "";

        public ControlAsistencia()
        {
            InitializeComponent();
            CargarSocios();
            CargarAsistenciasDeHoy();

            cmbSocioIngreso.ItemsSource = Socios;
            _vistaSocioIngreso = CollectionViewSource.GetDefaultView(cmbSocioIngreso.ItemsSource);
            _vistaSocioIngreso.Filter = FiltrarSocioIngreso;
            cmbSocioIngreso.AddHandler(TextBoxBase.TextChangedEvent,
                                       new TextChangedEventHandler(cmbSocioIngreso_TextChanged));

            cmbSocioHistorial.ItemsSource = Socios;
            _vistaSocioHistorial = CollectionViewSource.GetDefaultView(cmbSocioHistorial.ItemsSource);
            _vistaSocioHistorial.Filter = FiltrarSocioHistorial;
            cmbSocioHistorial.AddHandler(TextBoxBase.TextChangedEvent,
                                         new TextChangedEventHandler(cmbSocioHistorial_TextChanged));
            cmbSocioHistorial.SelectionChanged += cmbSocioHistorial_SelectionChanged;

            dgAsistenciasHoy.ItemsSource = AsistenciasHoy;
            dgHistorialSocio.ItemsSource = HistorialSocio;
        }

        private bool FiltrarSocioIngreso(object item)
        {
            if (string.IsNullOrEmpty(_textoBusquedaSocioIngreso)) return true;

            var socio = item as SocioVista;
            if (socio == null) return false;

            return socio.NombreCompleto.ToLower().Contains(_textoBusquedaSocioIngreso)
                || socio.Dni.ToLower().Contains(_textoBusquedaSocioIngreso);
        }

        private void cmbSocioIngreso_TextChanged(object sender, TextChangedEventArgs e)
        {
            var seleccionado = cmbSocioIngreso.SelectedItem as SocioVista;

            // Si el texto coincide exactamente con el socio ya elegido, es porque
            // alguien lo seleccionó: limpiamos el filtro y no abrimos nada.
            if (seleccionado != null && cmbSocioIngreso.Text == seleccionado.NombreConDni)
            {
                _textoBusquedaSocioIngreso = "";
                _vistaSocioIngreso?.Refresh();
                return;
            }

            _textoBusquedaSocioIngreso = cmbSocioIngreso.Text?.Trim().ToLower() ?? "";
            _vistaSocioIngreso?.Refresh();

            if (!cmbSocioIngreso.IsDropDownOpen)
            {
                cmbSocioIngreso.IsDropDownOpen = true;
            }
        }

        private bool FiltrarSocioHistorial(object item)
        {
            if (string.IsNullOrEmpty(_textoBusquedaSocioHistorial)) return true;

            var socio = item as SocioVista;
            if (socio == null) return false;

            return socio.NombreCompleto.ToLower().Contains(_textoBusquedaSocioHistorial)
                || socio.Dni.ToLower().Contains(_textoBusquedaSocioHistorial);
        }

        private void cmbSocioHistorial_TextChanged(object sender, TextChangedEventArgs e)
        {
            var seleccionado = cmbSocioHistorial.SelectedItem as SocioVista;

            // Si el texto coincide exactamente con el socio ya elegido, es porque
            // alguien lo seleccionó: limpiamos el filtro y no abrimos nada.
            if (seleccionado != null && cmbSocioHistorial.Text == seleccionado.NombreConDni)
            {
                _textoBusquedaSocioHistorial = "";
                _vistaSocioHistorial?.Refresh();
                ActualizarHistorial();
                return;
            }

            _textoBusquedaSocioHistorial = cmbSocioHistorial.Text?.Trim().ToLower() ?? "";
            _vistaSocioHistorial?.Refresh();
            ActualizarHistorial();

            if (!cmbSocioHistorial.IsDropDownOpen)
            {
                cmbSocioHistorial.IsDropDownOpen = true;
            }
        }

        private void cmbSocioHistorial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarHistorial();
        }

        // RF-10: historial de asistencias del socio resuelto por el buscador
        private void ActualizarHistorial()
        {
            HistorialSocio.Clear();
            txtSinHistorial.Visibility = Visibility.Collapsed;

            if (cmbSocioHistorial.SelectedItem is not SocioVista socio) return; // nada seleccionado

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

            if (cmbSocioIngreso.SelectedItem is not SocioVista socio)
            {
                MostrarMensaje("Debe seleccionar un socio.", esError: true);
                return;
            }

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
            cmbSocioIngreso.SelectedIndex = -1;
            cmbSocioIngreso.Text = string.Empty;
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