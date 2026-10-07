using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SGG;
using SGG.Formularios.Recepcionista;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaMisAlumnos : Window
    {
        public VentanaMisAlumnos()
        {
            InitializeComponent();
            CargarDatosDePrueba();
        }

        private void CargarDatosDePrueba()
        {
            // Carga solo los socios activos asignados al entrenador logueado (front/demo sobre DatosRecepDemo).
            var socios = DatosRecepDemo.ObtenerSocios()
                .Where(s => s.Activo && s.EntrenadorId.HasValue && s.EntrenadorId.Value == Sesion.UsuarioId)
                .ToList();

            var alumnos = socios
                .Select(s => new AlumnoItem
                {
                    SocioId = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    Dni = $"DNI {s.Dni}",
                    RutinaActual = null
                })
                .ToList();

            // Fallback demo: sin socios asignados al entrenador logueado se muestran alumnos de ejemplo.
            if (alumnos.Count == 0)
            {
                alumnos = new List<AlumnoItem>
                {
                    new AlumnoItem { SocioId = 1, NombreCompleto = "Juan Pérez", Dni = "DNI 40123456", RutinaActual = null },
                    new AlumnoItem { SocioId = 2, NombreCompleto = "María Gómez", Dni = "DNI 38654987",
                        RutinaActual = new RutinaListItem { Id = 1, SocioId = 2, Nombre = "Hipertrofia - Nivel Avanzado", SocioNombre = "María Gómez", DuracionSemanas = 8, FrecuenciaSemanal = 4, Objetivo = "Aumentar masa muscular general", Nivel = "Avanzado", Estado = "Activa" } },
                    new AlumnoItem { SocioId = 3, NombreCompleto = "Lucas Rodríguez", Dni = "DNI 42987123", RutinaActual = null },
                };
            }

            icSinRutina.ItemsSource = alumnos.Where(a => a.RutinaActual == null).ToList();
            icConRutina.ItemsSource = alumnos.Where(a => a.RutinaActual != null).ToList();
        }

        private void btnAsignar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var alumno = boton?.Tag as AlumnoItem;

            if (alumno != null)
            {
                var elegir = new VentanaElegirRutina(alumno.SocioId);
                elegir.Owner = this;
                elegir.ShowDialog();
            }
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var alumno = boton?.Tag as AlumnoItem;

            if (alumno?.RutinaActual != null)
            {
                var formulario = new VentanaGestionRutinas(alumno.RutinaActual);
                formulario.Owner = this;
                formulario.ShowDialog();
            }
        }
    }

    public class AlumnoItem
    {
        public int SocioId { get; set; }
        public string NombreCompleto { get; set; }
        public string Dni { get; set; }
        public RutinaListItem RutinaActual { get; set; }
    }
}