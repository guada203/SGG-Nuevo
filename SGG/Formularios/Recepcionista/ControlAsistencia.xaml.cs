using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Recepcionista
{
    public partial class ControlAsistencia : UserControl
    {
        private readonly ServicioAsistencias _servicioAsistencias = new();

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

            // RF-10: se consulta por Id (no por nombre) contra la base real.
            // El repositorio ya devuelve los registros ordenados de más reciente a más antiguo.
            var registros = _servicioAsistencias.ObtenerPorSocio(socio.Id)
                .Select(a => new AsistenciaHistorialVista
                {
                    Fecha = a.FechaHoraIngreso.ToString("dd/MM/yyyy"),
                    Hora = a.FechaHoraIngreso.ToString("HH:mm")
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
            foreach (var s in new ServicioSocios().ObtenerTodos())
            {
                Socios.Add(new SocioVista
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    Dni = s.Dni,
                    EstadoCuota = ServicioAsistencias.EstadoCuota(s)
                });
            }
        }

        private void CargarAsistenciasDeHoy()
        {
            foreach (var a in _servicioAsistencias.ObtenerDeHoy())
            {
                AsistenciasHoy.Add(new AsistenciaVista
                {
                    Socio = $"{a.Socio?.Nombre} {a.Socio?.Apellido}".Trim(),
                    Hora = a.FechaHoraIngreso.ToString("HH:mm"),
                    EstadoCuota = a.Socio != null ? ServicioAsistencias.EstadoCuota(a.Socio) : "Al día"
                });
            }
        }

        private void btnRegistrarIngreso_Click(object sender, RoutedEventArgs e)
        {
            OcultarMensaje();

            if (cmbSocioIngreso.SelectedItem is not SocioVista socio)
            {
                MostrarMensaje("Debe seleccionar un socio.", esError: true);
                return;
            }

            // RF-08/RF-09: el servicio valida existencia, alta y cuota al día,
            // y persiste la asistencia en la base real.
            var (exitoso, mensaje, asistencia) = _servicioAsistencias.RegistrarIngreso(socio.Id);
            if (!exitoso)
            {
                MostrarMensaje(mensaje, esError: true);
                return;
            }

            AsistenciasHoy.Add(new AsistenciaVista
            {
                Socio = socio.NombreCompleto,
                Hora = asistencia!.FechaHoraIngreso.ToString("HH:mm"),
                EstadoCuota = "Al día"
            });

            MostrarMensaje($"✔ Ingreso registrado correctamente para {socio.NombreCompleto}.", esError: false);
            cmbSocioIngreso.SelectedIndex = -1;
            cmbSocioIngreso.Text = string.Empty;
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