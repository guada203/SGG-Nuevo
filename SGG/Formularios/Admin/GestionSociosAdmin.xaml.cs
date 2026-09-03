using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SGG.Formularios.Login;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Admin
{
    public partial class GestionSociosAdmin : Window
    {
        private readonly ServicioSocios _servicioSocios = new();
        private ObservableCollection<SocioAdminVista> _todosLosSocios = new();
        public ObservableCollection<SocioAdminVista> Socios { get; set; } = new();

        public GestionSociosAdmin()
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
            CargarSocios();
            dgSocios.ItemsSource = Socios;
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
                    var membresias = new GestionMembresias();
                    membresias.Show();
                    this.Close();
                    break;
                case "Socios":
                    // Ya estamos acá, no hacemos nada
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

        private void CargarSocios()
        {
            var reales = _servicioSocios.ObtenerTodos();

            _todosLosSocios = new ObservableCollection<SocioAdminVista>(
                reales.Select(s => new SocioAdminVista
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    Dni = s.Dni,
                    Plan = s.Membresia?.TipoActividad.ToString() ?? "Sin plan",
                    Estado = s.Activo ? "Activo" : "Inactivo"
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

        private void txtBuscar_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string filtro = txtBuscar.Text.ToLower();
            Socios.Clear();

            var resultado = _todosLosSocios.Where(s =>
                s.NombreCompleto.ToLower().Contains(filtro) ||
                s.Dni.Contains(filtro));

            foreach (var s in resultado)
                Socios.Add(s);
        }

        private void btnBaja_Click(object sender, RoutedEventArgs e)
        {
            var boton = (System.Windows.Controls.Button)sender;
            int id = (int)boton.Tag;

            var socio = _todosLosSocios.FirstOrDefault(s => s.Id == id);
            if (socio == null) return;

            bool esActivo = socio.Estado == "Activo";
            string accion = esActivo ? "dar de baja" : "reactivar";

            var confirmacion = MessageBox.Show(
                $"¿Seguro que querés {accion} a {socio.NombreCompleto}?",
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

    public class SocioAdminVista
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public string Plan { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string AccionBaja => Estado == "Activo" ? "DAR DE BAJA" : "REACTIVAR";
    }
}
