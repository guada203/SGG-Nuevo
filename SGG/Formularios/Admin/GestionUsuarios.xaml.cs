using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Admin
{
    public partial class GestionUsuarios : UserControl
    {
        private readonly ServicioUsuarios _servicioUsuarios = new();
        private ObservableCollection<UsuarioVista> _todosLosUsuarios = new();
        public ObservableCollection<UsuarioVista> Usuarios { get; set; } = new();

        public GestionUsuarios()
        {
            InitializeComponent();
            CargarUsuarios();
            dgUsuarios.ItemsSource = Usuarios;
        }

        private void CargarUsuarios()
        {
            var usuariosReales = _servicioUsuarios.ObtenerTodos();

            _todosLosUsuarios = new ObservableCollection<UsuarioVista>(
                usuariosReales.Select(u => new UsuarioVista
                {
                    Id = u.Id,
                    Nombre = string.IsNullOrWhiteSpace(u.Apellido) ? u.Nombre : $"{u.Nombre} {u.Apellido}",
                    Dni = u.Dni ?? string.Empty,
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
                u.Dni.ToLower().Contains(filtro) ||
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
            var ventanaEdicion = new AltaUsuario((int)boton.Tag);
            ventanaEdicion.ShowDialog(); // se abre como modal, espera a que se cierre
            CargarUsuarios(); // al volver, recargamos la lista por si se modificaron datos
        }

        private void ToggleActivo_Click(object sender, RoutedEventArgs e)
        {
            var toggle = (System.Windows.Controls.Primitives.ToggleButton)sender;
            int id = (int)toggle.Tag;

            var usuario = _todosLosUsuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return;

            bool eraActivo = usuario.EsActivo;
            string accion = eraActivo ? "dar de baja" : "reactivar";

            // Protección 1: un administrador no puede desactivar su propia cuenta.
            if (eraActivo && Sesion.UsuarioId.HasValue && Sesion.UsuarioId.Value == id)
            {
                MessageBox.Show(
                    "No podés desactivar tu propia cuenta. Pedile a otro administrador.",
                    "Acción no permitida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                CargarUsuarios(); // revierte visualmente el toggle
                return;
            }

            // Protección 2: no se puede desactivar al último administrador activo.
            if (eraActivo && usuario.Rol == "Administrador")
            {
                int adminsActivos = _todosLosUsuarios.Count(u => u.Rol == "Administrador" && u.EsActivo);
                if (adminsActivos <= 1)
                {
                    MessageBox.Show(
                        "No se puede desactivar el último Administrador activo del sistema.",
                        "Acción no permitida",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    CargarUsuarios(); // revierte visualmente el toggle
                    return;
                }
            }

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
            public string Dni { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Rol { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public bool EsActivo => Estado == "Activo";
        }
}