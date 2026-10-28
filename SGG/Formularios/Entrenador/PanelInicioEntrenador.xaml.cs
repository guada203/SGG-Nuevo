using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Entrenador
{
    public partial class PanelInicioEntrenador : UserControl
    {
        private readonly ServicioSocios _servicioSocios = new();
        private readonly ServicioRutinas _servicioRutinas = new();
        public PanelInicioEntrenador()
        {
            InitializeComponent();

            txtFecha.Text = DateTime.Now.ToString(
                "dddd, dd 'de' MMMM 'de' yyyy",
                new System.Globalization.CultureInfo("es-AR")
            );

            CargarDatosDePrueba();
        }

        private void CargarDatosDePrueba()
        {

            // Obtener los socios activos asignados al entrenador actual.
            var socios = _servicioSocios.ObtenerTodos()
                .Where(s => s.Activo && s.EntrenadorId == Sesion.UsuarioId)
                .ToList();

            // Obtener los IDs de esos socios.
            var idsSocios = socios.Select(s => s.Id).ToHashSet();

            // Obtener las rutinas de los socios asignados.
            var rutinas = _servicioRutinas.ObtenerTodas()
                .Where(r => idsSocios.Contains(r.SocioId))
                .ToList();

            // Actualizar las estadísticas con datos reales.
            txtRutinasCreadas.Text = rutinas.Count.ToString();
            txtSociosAsignados.Text = socios.Count.ToString();
            txtRutinasActivas.Text = rutinas.Count(r => r.Estado == "Activa").ToString();
            txtRutinasCompletadas.Text = rutinas.Count(r => r.Estado == "Completada").ToString();



            dgRutinasRecientes.ItemsSource = rutinas
                .OrderByDescending(r => r.FechaAsignacion)
                .ThenByDescending(r => r.Id)
                .Take(5)
                .Select(r =>
                {
                    var socio = socios.FirstOrDefault(s => s.Id == r.SocioId);

                    return new RutinaResumen
                    {
                        Nombre = r.Nombre,
                        Socio = socio != null
                            ? $"{socio.Nombre} {socio.Apellido}"
                            : "Socio no encontrado",
                        Fecha = r.FechaAsignacion.ToString("dd MMM yyyy"),
                        Estado = r.Estado
                    };
                })
                .ToList();



            icSociosSinRutina.ItemsSource = socios
                .Where(s => !rutinas.Any(r =>
                    r.SocioId == s.Id && r.Estado == "Activa"))
                .Select(s => new SocioPendiente
                {
                    SocioId = s.Id,
                    Nombre = $"{s.Nombre} {s.Apellido}".Trim(),
                    Meta = "Sin rutina activa"
                })
                .ToList();

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