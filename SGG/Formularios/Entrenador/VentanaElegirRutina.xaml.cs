using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaElegirRutina : Window
    {
        private readonly int _socioIdDestino;
        private readonly ServicioRutinas _servicioRutinas = new();
        private readonly ServicioSocios _servicioSocios = new();


        public VentanaElegirRutina(int socioIdDestino)
        {
            InitializeComponent();
            _socioIdDestino = socioIdDestino;
            CargarRutinasDisponibles();
        }


        private void CargarRutinasDisponibles()
        {
            // Obtenemos los socios activos asignados al entrenador.
            var idsSocios = _servicioSocios.ObtenerTodos()
                .Where(s => s.Activo && s.EntrenadorId == Sesion.UsuarioId)
                .Select(s => s.Id)
                .ToHashSet();

            // Mostramos solamente las rutinas de esos socios.
            var rutinasDb = _servicioRutinas.ObtenerTodas()
                .Where(r => idsSocios.Contains(r.SocioId))
                .ToList();

            // Convertimos las rutinas en objetos que nuestra
            // interfaz ya sabe mostrar.
            var rutinas = rutinasDb.Select(r => new RutinaListItem
            {
                Id = r.Id,
                SocioId = r.SocioId,
                Nombre = r.Nombre,
                DuracionSemanas = r.DuracionSemanas,
                FrecuenciaSemanal = r.FrecuenciaSemanal,
                Objetivo = r.Objetivo,
                Nivel = r.Nivel,
                Estado = r.Estado,
                CantidadEjercicios = r.Ejercicios.Count,

                SocioNombre = r.Socio != null
                    ? $"{r.Socio.Nombre} {r.Socio.Apellido}"
                    : "Socio no encontrado"

            }).ToList();

            // Mostramos las rutinas reales en la ventana.
            icRutinasDisponibles.ItemsSource = rutinas;
        }


        private void btnAsignar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var plantilla = boton?.Tag as RutinaListItem;

            if (plantilla != null)
            {
                var formulario = new VentanaGestionRutinas(plantilla, _socioIdDestino);
                formulario.Owner = this;
                formulario.ShowDialog();
                this.Close();
            }
        }

        private void btnCrearNueva_Click(object sender, RoutedEventArgs e)
        {
            var formulario = new VentanaGestionRutinas(_socioIdDestino);
            formulario.Owner = this;
            formulario.ShowDialog();
            this.Close();
        }
    }
}