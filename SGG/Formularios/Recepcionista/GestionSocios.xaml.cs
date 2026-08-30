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

namespace SGG.Formularios.Recepcionista
{
    public partial class GestionSocios : Window
    {
        private ObservableCollection<SocioVista> _todosLosSocios = new();
        public ObservableCollection<SocioVista> Socios { get; set; } = new();

        public GestionSocios()
        {
            InitializeComponent();
            CargarDatosDeEjemplo();
            dgSocios.ItemsSource = Socios;
        }

        private void CargarDatosDeEjemplo()
        {
            // TODO: reemplazar por datos reales desde SGG.Logica / EF Core
            _todosLosSocios.Add(new SocioVista { Id = 1, NombreCompleto = "Carolina Méndez", Dni = "38456123", Plan = "Musculación", Estado = "Activo", Vence = "31/08/2026" });
            _todosLosSocios.Add(new SocioVista { Id = 2, NombreCompleto = "Tomás Restrepo", Dni = "40123456", Plan = "Funcional", Estado = "Activo", Vence = "31/08/2026" });
            _todosLosSocios.Add(new SocioVista { Id = 3, NombreCompleto = "Lucía Vargas", Dni = "35987654", Plan = "Combinado", Estado = "Inactivo", Vence = "15/07/2026" });

            foreach (var s in _todosLosSocios)
                Socios.Add(s);

            txtCantidadSocios.Text = $"{_todosLosSocios.Count} socios registrados";
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

        private void btnNuevoSocio_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Acá vamos a abrir el formulario de alta de socio.");
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = (System.Windows.Controls.Button)sender;
            MessageBox.Show($"Editar socio Id: {boton.Tag}");
        }

        private void btnDarBaja_Click(object sender, RoutedEventArgs e)
        {
            var boton = (System.Windows.Controls.Button)sender;
            MessageBox.Show($"Dar de baja socio Id: {boton.Tag}");
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            var dashboard = new VentanaPrincipalRecepcionista();
            dashboard.Show();
            this.Close();
        }

        private void btnPagos_Click(object sender, RoutedEventArgs e)
        {
            var registrarPago = new RegistrarPago();
            registrarPago.Show();
            this.Close();
        }

        private void btnAsistencia_Click(object sender, RoutedEventArgs e)
        {
            var controlAsistencia = new ControlAsistencia();
            controlAsistencia.Show();
            this.Close();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var ventanaRol = new VentanaSeleccionRol();
            ventanaRol.Show();
            this.Close();
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
    }
}
