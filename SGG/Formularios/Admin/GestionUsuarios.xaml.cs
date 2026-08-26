using SGG.Formularios.Login;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class GestionUsuarios : Window
    {
        private ObservableCollection<UsuarioVista> _todosLosUsuarios = new();
        public ObservableCollection<UsuarioVista> Usuarios { get; set; } = new();

        public GestionUsuarios()
        {
            InitializeComponent();
            CargarDatosDeEjemplo();
            dgUsuarios.ItemsSource = Usuarios;
        }

        private void CargarDatosDeEjemplo()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica cuando conectemos la BD
            _todosLosUsuarios.Add(new UsuarioVista { Id = 1, Nombre = "Carlos Rueda", Email = "carlos.r@sgg.com", Rol = "Administrador", Estado = "Activo" });
            _todosLosUsuarios.Add(new UsuarioVista { Id = 2, Nombre = "Mónica Herrera", Email = "monica.h@sgg.com", Rol = "Recepcionista", Estado = "Activo" });
            _todosLosUsuarios.Add(new UsuarioVista { Id = 3, Nombre = "Andrés Mora", Email = "andres.m@sgg.com", Rol = "Entrenador", Estado = "Activo" });
            _todosLosUsuarios.Add(new UsuarioVista { Id = 4, Nombre = "María Salcedo", Email = "maria.s@sgg.com", Rol = "Entrenador", Estado = "Activo" });

            foreach (var u in _todosLosUsuarios)
                Usuarios.Add(u);

            ActualizarContadores();
        }

        private void ActualizarContadores()
        {
            txtCantidadUsuarios.Text = $"{_todosLosUsuarios.Count} usuarios registrados en el sistema";
            txtCantAdmins.Text = _todosLosUsuarios.Count(u => u.Rol == "Administrador").ToString();
            txtCantRecepcionistas.Text = _todosLosUsuarios.Count(u => u.Rol == "Recepcionista").ToString();
            txtCantEntrenadores.Text = _todosLosUsuarios.Count(u => u.Rol == "Entrenador").ToString();
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
            MessageBox.Show("Acá vamos a abrir el formulario de alta de usuario.");
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = (System.Windows.Controls.Button)sender;
            MessageBox.Show($"Editar usuario Id: {boton.Tag}");
        }

        private void btnDarBaja_Click(object sender, RoutedEventArgs e)
        {
            var boton = (System.Windows.Controls.Button)sender;
            MessageBox.Show($"Dar de baja usuario Id: {boton.Tag}");
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            var dashboard = new VentanaPrincipalAdmin();
            dashboard.Show();
            this.Close();
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {

            var reportes = new Reportes();
            reportes.Show();
            this.Close();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var ventanaRol = new VentanaSeleccionRol();
            ventanaRol.Show();
            this.Close();
        }
    }

    public class UsuarioVista
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
