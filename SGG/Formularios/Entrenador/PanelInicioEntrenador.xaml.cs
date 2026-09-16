using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SGG.Formularios.Entrenador
{
    public partial class PanelInicioEntrenador : UserControl
    {
        public PanelInicioEntrenador()
        {
            InitializeComponent();
            CargarDatosDePrueba();
        }

        private void CargarDatosDePrueba()
        {
            txtRutinasCreadas.Text = "24";
            txtSociosAsignados.Text = "18";
            txtRutinasActivas.Text = "15";
            txtRutinasCompletadas.Text = "9";

            dgRutinasRecientes.ItemsSource = new List<RutinaResumen>
            {
                new RutinaResumen { Nombre = "Hipertrofia", Socio = "Mateo Rodríguez", Fecha = "21 Oct 2024", Estado = "Activa" },
                new RutinaResumen { Nombre = "Acondicionamiento", Socio = "Valeria Espinoza", Fecha = "19 Oct 2024", Estado = "Activa" },
                new RutinaResumen { Nombre = "Fuerza", Socio = "Ignacio Silva", Fecha = "18 Oct 2024", Estado = "Completada" },
            };

            icSociosSinRutina.ItemsSource = new List<SocioPendiente>
            {
                new SocioPendiente { SocioId = 12, Nombre = "Sofía Martínez", Meta = "Ingresó hoy" },
                new SocioPendiente { SocioId = 15, Nombre = "Lucas Domínguez", Meta = "Hace 2 días" },
                new SocioPendiente { SocioId = 19, Nombre = "Martina Vega", Meta = "Hace 3 días" },
            };
        }

        private void btnCrearRutina_Click(object sender, RoutedEventArgs e)
        {
            var gestionRutinas = new VentanaGestionRutinas();
            gestionRutinas.Owner = Window.GetWindow(this);
            gestionRutinas.ShowDialog();
        }

        private void btnAsignarDesdeInicio_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var socio = boton?.Tag as SocioPendiente;

            if (socio != null)
            {
                var elegir = new VentanaElegirRutina(socio.SocioId);
                elegir.Owner = Window.GetWindow(this);
                elegir.ShowDialog();
            }
        }
    }

    public class RutinaResumen
    {
        public string Nombre { get; set; } = string.Empty;
        public string Socio { get; set; } = string.Empty;
        public string Fecha { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class SocioPendiente
    {
        public int SocioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
    }
}