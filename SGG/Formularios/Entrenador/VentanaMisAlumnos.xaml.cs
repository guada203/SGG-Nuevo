using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SGG;
using SGG.Formularios.Recepcionista;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaMisAlumnos : Window
    {
        private readonly ServicioSocios _servicioSocios = new();
        private readonly ServicioRutinas _servicioRutinas = new();
        private List<AlumnoItem> _todosLosAlumnos = new();
        public VentanaMisAlumnos()
        {
            InitializeComponent();
            CargarDatosDePrueba();
        }



        private void CargarDatosDePrueba()
        {
            // Obtener los socios activos del entrenador que inició sesión.
            var socios = _servicioSocios.ObtenerTodos()
                .Where(s => s.Activo && s.EntrenadorId == Sesion.UsuarioId)
                .ToList();

            // Obtener las rutinas reales guardadas en SQL Server.
            var rutinas = _servicioRutinas.ObtenerTodas();

            // Preparar los alumnos para mostrarlos en las tarjetas.
            var alumnos = socios.Select(s =>
            {
                // Buscar una rutina activa que pertenezca a este socio.
                var rutina = rutinas
                    .Where(r => r.SocioId == s.Id && r.Estado == "Activa")
                    .OrderByDescending(r => r.FechaAsignacion)
                    .ThenByDescending(r => r.Id)
                    .FirstOrDefault();

                return new AlumnoItem
                {
                    SocioId = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido}".Trim(),
                    Dni = $"DNI {s.Dni}",

                    // Convertir la rutina al formato que utiliza nuestra interfaz.
                    RutinaActual = rutina == null ? null : new RutinaListItem
                    {
                        Id = rutina.Id,
                        SocioId = rutina.SocioId,
                        Nombre = rutina.Nombre,
                        SocioNombre = $"{s.Nombre} {s.Apellido}".Trim(),
                        DuracionSemanas = rutina.DuracionSemanas,
                        FrecuenciaSemanal = rutina.FrecuenciaSemanal,
                        Objetivo = rutina.Objetivo,
                        Nivel = rutina.Nivel,
                        Estado = rutina.Estado,
                        CantidadEjercicios = rutina.Ejercicios.Count
                    }
                };
            }).ToList();

            _todosLosAlumnos = alumnos;
            FiltrarAlumnos();
        }

        private void txtBuscarAlumno_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarAlumnos();
        }

        private void FiltrarAlumnos()
        {
            string busqueda = txtBuscarAlumno.Text.Trim();

            var alumnosFiltrados = _todosLosAlumnos
                .Where(a =>
                    a.NombreCompleto.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    a.Dni.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
                .ToList();

            icSinRutina.ItemsSource = alumnosFiltrados
                .Where(a => a.RutinaActual == null)
                .ToList();

            icConRutina.ItemsSource = alumnosFiltrados
                .Where(a => a.RutinaActual != null)
                .ToList();
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

                // Actualizar las tarjetas al volver de la ventana de asignación.
                CargarDatosDePrueba();
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

                // Actualizar las tarjetas al volver de editar una rutina.
                CargarDatosDePrueba();
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