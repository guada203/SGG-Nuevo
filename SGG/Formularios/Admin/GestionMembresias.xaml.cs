using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SGG.Formularios.Login;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Admin
{
    public partial class GestionMembresias : Window
    {
        private readonly ServicioMembresias _servicioMembresias = new();
        private ObservableCollection<MembresiaVista> _todasLasMembresias = new();
        public ObservableCollection<MembresiaVista> Membresias { get; set; } = new();

        public GestionMembresias()
        {
            InitializeComponent();
            menuLateral.OpcionSeleccionada += ManejarOpcionSeleccionada;
            try
            {
                menuLateral.ConfigurarRol("Administrador");
            }
            catch
            {
                // Ignorar errores de configuración del menú
            }
            CargarMembresias();
            dgMembresias.ItemsSource = Membresias;
        }

        private void ManejarOpcionSeleccionada(string opcion)
        {
            switch (opcion)
            {
                case "Inicio":
                    var dashboard = new VentanaPrincipalAdmin();
                    dashboard.Show();
                    this.Close();
                    break;
                case "Usuarios":
                    var usuarios = new GestionUsuarios();
                    usuarios.Show();
                    this.Close();
                    break;
                case "Membresias":
                    // Ya estamos acá, no hacemos nada
                    break;
                case "Socios":
                    var socios = new GestionSociosAdmin();
                    socios.Show();
                    this.Close();
                    break;
                case "Reportes":
                    var reportes = new Reportes();
                    reportes.Show();
                    this.Close();
                    break;
                case "CerrarSesion":
                    var ventanaRol = new VentanaSeleccionRol();
                    ventanaRol.Show();
                    this.Close();
                    break;
            }
        }

        private void CargarMembresias()
        {
            var reales = _servicioMembresias.ObtenerTodas();

            _todasLasMembresias = new ObservableCollection<MembresiaVista>(
                reales.Select(m => new MembresiaVista
                {
                    Id = m.Id,
                    Actividad = m.TipoActividad.ToString(),
                    Precio = m.Precio.ToString("C"),
                    Vence = m.FechaVencimiento.ToShortDateString(),
                    Estado = m.Vigente ? "Vigente" : "No vigente"
                })
            );

            Membresias.Clear();
            foreach (var m in _todasLasMembresias)
                Membresias.Add(m);

            ActualizarContador();
        }

        private void ActualizarContador()
        {
            txtCantidadMembresias.Text = $"{_todasLasMembresias.Count} membresías registradas";
        }

        private void txtBuscar_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string filtro = txtBuscar.Text.ToLower();
            Membresias.Clear();

            var resultado = _todasLasMembresias.Where(m =>
                m.Actividad.ToLower().Contains(filtro) ||
                m.Estado.ToLower().Contains(filtro));

            foreach (var m in resultado)
                Membresias.Add(m);
        }

        private void btnNuevaMembresia_Click(object sender, RoutedEventArgs e)
        {
            var ventanaAlta = new AltaMembresia();
            if (ventanaAlta.ShowDialog() == true)
                CargarMembresias();
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = (System.Windows.Controls.Button)sender;
            int id = (int)boton.Tag;

            var ventanaAlta = new AltaMembresia(id);
            if (ventanaAlta.ShowDialog() == true)
                CargarMembresias();
        }

        private void btnDarBaja_Click(object sender, RoutedEventArgs e)
        {
            var boton = (System.Windows.Controls.Button)sender;
            int id = (int)boton.Tag;

            var membresia = _todasLasMembresias.FirstOrDefault(m => m.Id == id);
            if (membresia == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Seguro que querés dar de baja la membresía {membresia.Actividad}? " +
                "No estará disponible al registrar nuevos socios.",
                "Confirmar baja",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            _servicioMembresias.DarDeBaja(id);
            CargarMembresias();
        }
    }

    public class MembresiaVista
    {
        public int Id { get; set; }
        public string Actividad { get; set; } = string.Empty;
        public string Precio { get; set; } = string.Empty;
        public string Vence { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
