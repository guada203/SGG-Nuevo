using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaListaRutinas : UserControl
    {
        private List<RutinaListItem> _todasLasRutinas;
        private string _filtroActual = "Todas";
        private readonly ServicioRutinas _servicioRutinas = new();
        private readonly ServicioSocios _servicioSocios = new();

        public VentanaListaRutinas()
        {
            InitializeComponent();
            CargarRutinas();
            AplicarFiltros();
        }


        private void CargarRutinas()
        {
            // Obtener los socios asignados al entrenador que inició sesión.
            var idsSocios = _servicioSocios.ObtenerTodos()
                .Where(s => s.Activo && s.EntrenadorId == Sesion.UsuarioId)
                .Select(s => s.Id)
                .ToHashSet();

            // Obtener solamente las rutinas de esos socios.
            var rutinas = _servicioRutinas.ObtenerTodas()
                .Where(r => idsSocios.Contains(r.SocioId))
                .ToList();

            // Convertimos las entidades en objetos que
            // nuestra interfaz ya sabe mostrar.
            _todasLasRutinas = rutinas.Select(r => new RutinaListItem
            {
                Id = r.Id,
                SocioId = r.SocioId,
                Nombre = r.Nombre,

                // Obtenemos el nombre del socio relacionado.
                SocioNombre = r.Socio != null
                    ? $"{r.Socio.Nombre} {r.Socio.Apellido}"
                    : "Socio no encontrado",

                FechaCreacionTexto = $"Asignada el {r.FechaAsignacion:dd/MM/yyyy}",
                DuracionSemanas = r.DuracionSemanas,
                FrecuenciaSemanal = r.FrecuenciaSemanal,
                Objetivo = r.Objetivo,
                Nivel = r.Nivel,
                CantidadEjercicios = r.Ejercicios.Count,
                Estado = r.Estado

            }).ToList();
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
            formulario.Owner = Window.GetWindow(this);

            // Si se guardó correctamente, actualizamos la lista.
            if (formulario.ShowDialog() == true)
            {
                CargarRutinas();
                AplicarFiltros();
            }
        }


        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var rutina = boton?.Tag as RutinaListItem;

            if (rutina != null)
            {
                var formulario = new VentanaGestionRutinas(rutina);
                formulario.Owner = Window.GetWindow(this);

                // Si el entrenador guardó los cambios correctamente,
                // volvemos a consultar las rutinas en SQL Server.
                if (formulario.ShowDialog() == true)
                {
                    CargarRutinas();
                    AplicarFiltros();
                }
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
                "Completada" => "#3B82F6",
                _ => "#888888"
            };
    }
}