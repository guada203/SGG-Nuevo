using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SGG.Formularios.Recepcionista
{
    public partial class GestionSocios : UserControl
    {
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
            // TODO integración BD: en la fase de conexión esto se reemplaza por
            // ServicioSocios.ObtenerTodos() (SGG.Logica / SGG.Datos).
            var sociosDemo = DatosRecepDemo.ObtenerSocios();

            _todosLosSocios = new ObservableCollection<SocioVista>(
                sociosDemo.Select(s => new SocioVista
                {
                    Id = s.Id,
                    NombreCompleto = s.NombreCompleto,
                    Dni = s.Dni,
                    Plan = s.TipoMembresia,
                    Estado = s.Activo ? "Activo" : "Inactivo",
                    Vence = s.FechaVencimiento < DateTime.Today
                        ? "Vencida"
                        : s.FechaVencimiento.ToShortDateString(),
                    EstadoCuota = DatosRecepDemo.EstadoCuota(s)
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

            var demo = DatosRecepDemo.Socios.FirstOrDefault(s => s.Id == id);
            if (demo == null) return;

            var ventana = new AltaSocio(demo);
            if (ventana.ShowDialog() == true)
                CargarSocios();
        }

        private void btnBaja_Click(object sender, RoutedEventArgs e)
        {
            var boton = (Button)sender;
            int id = (int)boton.Tag;

            var vista = _todosLosSocios.FirstOrDefault(s => s.Id == id);
            var demo = DatosRecepDemo.Socios.FirstOrDefault(s => s.Id == id);
            if (vista == null || demo == null) return;

            bool esActivo = demo.Activo;
            string accion = esActivo ? "dar de baja" : "reactivar";

            var confirmacion = MessageBox.Show(
                $"¿Seguro que querés {accion} a {vista.NombreCompleto}?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            // RF-04: alternar el estado en la lista demo y refrescar la fila en vivo
            demo.Activo = !demo.Activo;
            vista.Estado = demo.Activo ? "Activo" : "Inactivo";

            ActualizarContador();
            AplicarFiltro();
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
    }
}