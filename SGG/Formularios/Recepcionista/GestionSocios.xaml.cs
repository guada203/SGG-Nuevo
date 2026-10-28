using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SGG.Dominio.Entidades;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Recepcionista
{
    public partial class GestionSocios : UserControl
    {
        private readonly ServicioSocios _servicioSocios = new();
        private readonly Dictionary<int, Socio> _sociosPorId = new();

        private ObservableCollection<SocioVista> _todosLosSocios = new();
        public ObservableCollection<SocioVista> Socios { get; set; } = new();

        public GestionSocios()
        {
            InitializeComponent();
            CargarSocios();
            dgSocios.ItemsSource = Socios;
        }

        private void CargarSocios()
        {
            var reales = _servicioSocios.ObtenerTodos();

            _sociosPorId.Clear();
            foreach (var s in reales)
                _sociosPorId[s.Id] = s;

            _todosLosSocios = new ObservableCollection<SocioVista>(
                reales.Select(s => new SocioVista
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    Dni = s.Dni,
                    Plan = s.Membresia?.TipoActividad.ToString() ?? "Sin plan",
                    Estado = s.Activo ? "Activo" : "Inactivo",
                    Vence = !s.FechaVencimiento.HasValue
                        ? "Sin cuota"
                        : s.FechaVencimiento.Value.Date < DateTime.Today
                            ? "Vencida"
                            : s.FechaVencimiento.Value.ToShortDateString(),
                    EstadoCuota = ServicioAsistencias.EstadoCuota(s)
                })
            );

            Socios.Clear();
            foreach (var s in _todosLosSocios)
                Socios.Add(s);

            ActualizarContador();
        }

        private void ActualizarContador()
        {
            int activos = _todosLosSocios.Count(s => s.Estado == "Activo");
            txtCantidadSocios.Text = $"{_todosLosSocios.Count} socios registrados ({activos} activos)";
        }

        private void AplicarFiltro()
        {
            string filtro = txtBuscar.Text.Trim().ToLower();
            Socios.Clear();

            var resultado = _todosLosSocios.Where(s =>
                string.IsNullOrEmpty(filtro) ||
                s.NombreCompleto.ToLower().Contains(filtro) ||
                s.Dni.Contains(filtro));

            foreach (var s in resultado)
                Socios.Add(s);
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltro();

            hintBuscar.Visibility = string.IsNullOrEmpty(txtBuscar.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void btnNuevoSocio_Click(object sender, RoutedEventArgs e)
        {
            var ventanaAlta = new AltaSocio();
            if (ventanaAlta.ShowDialog() == true)
                CargarSocios();
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = (Button)sender;
            int id = (int)boton.Tag;

            if (!_sociosPorId.TryGetValue(id, out var socio))
                return;

            var ventana = new AltaSocio(socio);
            if (ventana.ShowDialog() == true)
                CargarSocios();
        }

        private void btnBaja_Click(object sender, RoutedEventArgs e)
        {
            var boton = (Button)sender;
            int id = (int)boton.Tag;

            var vista = _todosLosSocios.FirstOrDefault(s => s.Id == id);
            if (!_sociosPorId.TryGetValue(id, out var socio) || vista == null)
                return;

            bool esActivo = socio.Activo;
            string accion = esActivo ? "dar de baja" : "reactivar";

            var confirmacion = MessageBox.Show(
                $"¿Seguro que querés {accion} a {vista.NombreCompleto}?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            var resultado = esActivo
                ? _servicioSocios.DarDeBaja(id)
                : _servicioSocios.Reactivar(id);

            if (!resultado.Exitoso)
            {
                MessageBox.Show(resultado.Mensaje, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CargarSocios();
        }
    }

    public class SocioVista
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Vence { get; set; } = string.Empty;
        public string EstadoCuota { get; set; } = string.Empty;
        public string AccionBaja => Estado == "Activo" ? "DAR DE BAJA" : "REACTIVAR";

        // Mismo formato que el combo del Entrenador: "Juan Pérez - DNI 40123456"
        public string NombreConDni => $"{NombreCompleto} - DNI {Dni}";

        public override string ToString() => NombreCompleto;
    }
}