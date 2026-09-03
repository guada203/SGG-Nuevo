using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SGG.Formularios.Login;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaListaRutinas : Window
    {
        private List<RutinaListItem> _todasLasRutinas;
        private string _filtroActual = "Todas";

        public VentanaListaRutinas()
        {
            InitializeComponent();
            CargarDatosDePrueba();
            AplicarFiltros();
        }

        private void CargarDatosDePrueba()
        {
            _todasLasRutinas = new List<RutinaListItem>
            {
                new RutinaListItem { Id = 1, Nombre = "Hipertrofia - Nivel Avanzado", SocioId = 1, SocioNombre = "Mateo Rodríguez", FechaCreacionTexto = "Creada el 21 Oct 2024", DuracionSemanas = 8, FrecuenciaSemanal = 4, Objetivo = "Aumentar masa muscular general", Nivel = "Avanzado", CantidadEjercicios = 12, Estado = "Activa" },
                new RutinaListItem { Id = 2, Nombre = "Acondicionamiento General", SocioId = 2, SocioNombre = "Valeria Espinoza", FechaCreacionTexto = "Creada el 19 Oct 2024", DuracionSemanas = 6, FrecuenciaSemanal = 3, Objetivo = "Mejorar resistencia cardiovascular", Nivel = "Principiante", CantidadEjercicios = 8, Estado = "Activa" },
                new RutinaListItem { Id = 3, Nombre = "Fuerza Funcional", SocioId = 3, SocioNombre = "Ignacio Silva", FechaCreacionTexto = "Creada el 18 Oct 2024", DuracionSemanas = 10, FrecuenciaSemanal = 4, Objetivo = "Ganar fuerza en levantamientos básicos", Nivel = "Intermedio", CantidadEjercicios = 10, Estado = "Pendiente" },
                new RutinaListItem { Id = 4, Nombre = "Definición Muscular", SocioId = 1, SocioNombre = "Camila Peralta", FechaCreacionTexto = "Creada el 15 Oct 2024", DuracionSemanas = 4, FrecuenciaSemanal = 5, Objetivo = "Reducir grasa manteniendo masa muscular", Nivel = "Avanzado", CantidadEjercicios = 7, Estado = "Completada" },
                new RutinaListItem { Id = 5, Nombre = "Rehabilitación Lumbar", SocioId = 2, SocioNombre = "Daniel Fuentes", FechaCreacionTexto = "Creada el 12 Oct 2024", DuracionSemanas = 12, FrecuenciaSemanal = 2, Objetivo = "Recuperar movilidad y fuerza lumbar", Nivel = "Principiante", CantidadEjercicios = 6, Estado = "Activa" },
                new RutinaListItem { Id = 6, Nombre = "Cardio HIIT Intensivo", SocioId = 3, SocioNombre = "Sofía Martínez", FechaCreacionTexto = "Creada el 10 Oct 2024", DuracionSemanas = 6, FrecuenciaSemanal = 3, Objetivo = "Quemar grasa con entrenamiento interválico", Nivel = "Intermedio", CantidadEjercicios = 9, Estado = "Pendiente" },
            };
        }

        private void AplicarFiltros()
        {
            var resultado = _todasLasRutinas.AsEnumerable();

            if (_filtroActual != "Todas")
            {
                resultado = resultado.Where(r => r.Estado == _filtroActual);
            }

            string textoBuscado = txtBuscar.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(textoBuscado))
            {
                resultado = resultado.Where(r =>
                    r.Nombre.ToLower().Contains(textoBuscado) ||
                    r.SocioNombre.ToLower().Contains(textoBuscado));
            }

            icRutinas.ItemsSource = resultado.ToList();
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void btnFiltro_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            _filtroActual = boton?.Tag as string ?? "Todas";

            btnFiltroTodas.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#1A1A1A");
            btnFiltroActivas.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#1A1A1A");
            btnFiltroPendientes.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#1A1A1A");
            btnFiltroCompletadas.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#1A1A1A");

            if (boton != null)
            {
                boton.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#A855F7");
            }

            AplicarFiltros();
        }

        private void btnCrearRutina_Click(object sender, RoutedEventArgs e)
        {
            var formulario = new VentanaGestionRutinas();
            formulario.Owner = this;
            formulario.ShowDialog();
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var rutina = boton?.Tag as RutinaListItem;

            if (rutina != null)
            {
                var formulario = new VentanaGestionRutinas(rutina);
                formulario.Owner = this;
                formulario.ShowDialog();
            }
        }

        private void btnVerDetalle_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var rutina = boton?.Tag as RutinaListItem;

            if (rutina != null)
            {
                MessageBox.Show(
                    $"Rutina: {rutina.Nombre}\nSocio: {rutina.SocioNombre}\nDuración: {rutina.DuracionSemanas} semanas\nEjercicios: {rutina.CantidadEjercicios}\nEstado: {rutina.Estado}",
                    "Detalle de la rutina");
            }
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            var inicio = new VentanaPrincipalEntrenador();
            inicio.Show();
            this.Close();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            var ventanaRol = new VentanaSeleccionRol();
            ventanaRol.Show();
            this.Close();
        }
    }

    public class RutinaListItem
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public string Nombre { get; set; }
        public string SocioNombre { get; set; }
        public string FechaCreacionTexto { get; set; }
        public int DuracionSemanas { get; set; }
        public int FrecuenciaSemanal { get; set; }
        public string Objetivo { get; set; }
        public string Nivel { get; set; }
        public int CantidadEjercicios { get; set; }
        public string Estado { get; set; }

        public string ColorEstado =>
            Estado switch
            {
                "Activa" => "#22C55E",
                "Pendiente" => "#F59E0B",
                "Completada" => "#3B82F6",
                _ => "#888888"
            };
    }
}