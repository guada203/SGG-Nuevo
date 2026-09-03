using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SGG.Formularios.Login;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Admin
{
    public partial class GestionUsuarios : Window
    {
        private readonly ServicioUsuarios _servicioUsuarios = new();
        private ObservableCollection<UsuarioVista> _todosLosUsuarios = new();
        public ObservableCollection<UsuarioVista> Usuarios { get; set; } = new();

        public GestionUsuarios()
        {
            InitializeComponent();
            // Suscribirse a los eventos del menú lateral para navegación
            menuLateral.OpcionSeleccionada += ManejarOpcionSeleccionada;
            // Mostrar opciones de administrador en el menú lateral
            try
            {
                menuLateral.ConfigurarRol("Administrador");
            }
            catch
            {
                // Ignorar errores de configuración del menú
            }
            CargarUsuarios();
            dgUsuarios.ItemsSource = Usuarios;
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

        private void CargarUsuarios()
        {
            var usuariosReales = _servicioUsuarios.ObtenerTodos();

            _todosLosUsuarios = new ObservableCollection<UsuarioVista>(
                usuariosReales.Select(u => new UsuarioVista
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = u.Rol?.Nombre ?? string.Empty,
                    Estado = u.Activo ? "Activo" : "Inactivo"
                })
            );

            Usuarios.Clear();
            foreach (var u in _todosLosUsuarios)
                Usuarios.Add(u);

            ActualizarContadores();
        }

        private void ActualizarContadores()
        {
            txtCantidadUsuarios.Text = $"{_todosLosUsuarios.Count} usuarios registrados en el sistema";
            txtCantAdmins.Text = _todosLosUsuarios.Count(u => u.Rol == "Administrador" && u.Estado == "Activo").ToString();
            txtCantRecepcionistas.Text = _todosLosUsuarios.Count(u => u.Rol == "Recepcionista" && u.Estado == "Activo").ToString();
            txtCantEntrenadores.Text = _todosLosUsuarios.Count(u => u.Rol == "Entrenador" && u.Estado == "Activo").ToString();
        }

        private void txtBuscar_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string filtro = txtBuscar.Text.ToLower();
            Usuarios.Clear();

            var resultado = _todosLosUsuarios.Where(u =>
                u.Nombre.ToLower().Contains(filtro) ||
                u.Email.ToLower().Contains(filtro));

            foreach (var u in resultado)
                Usuarios.Add(u);
        }

        private void btnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            var ventanaAlta = new AltaUsuario();
            ventanaAlta.ShowDialog(); // se abre como modal, espera a que se cierre
            CargarUsuarios(); // al volver, recargamos la lista por si se agregó uno nuevo
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = (System.Windows.Controls.Button)sender;
            MessageBox.Show($"Editar usuario Id: {boton.Tag} (pendiente de implementar).");
        }

        private void ToggleActivo_Click(object sender, RoutedEventArgs e)
        {
            var toggle = (System.Windows.Controls.Primitives.ToggleButton)sender;
            int id = (int)toggle.Tag;

            var usuario = _todosLosUsuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return;

            bool eraActivo = usuario.EsActivo;
            string accion = eraActivo ? "dar de baja" : "reactivar";

            var confirmacion = MessageBox.Show(
                $"¿Seguro que querés {accion} a {usuario.Nombre}?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
            {
                CargarUsuarios(); // revierte visualmente el toggle si cancela
                return;
            }

            if (eraActivo)
                _servicioUsuarios.DarDeBaja(id);
            else
                _servicioUsuarios.Reactivar(id);

            CargarUsuarios();
        }
    }

    public class UsuarioVista
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public bool EsActivo => Estado == "Activo";
    }
}