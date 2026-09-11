using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaElegirRutina : Window
    {
        private readonly int _socioIdDestino;

        public VentanaElegirRutina(int socioIdDestino)
        {
            InitializeComponent();
            _socioIdDestino = socioIdDestino;
            CargarRutinasDisponibles();
        }

        private void CargarRutinasDisponibles()
        {
            // Datos de prueba -- cuando conectemos la base, esto va a traer
            // todas las rutinas reales que existan (de cualquier socio).
            var rutinas = new List<RutinaListItem>
            {
                new RutinaListItem { Id = 1, Nombre = "Hipertrofia - Nivel Avanzado", DuracionSemanas = 8, FrecuenciaSemanal = 4, Objetivo = "Aumentar masa muscular general", Nivel = "Avanzado" },
                new RutinaListItem { Id = 2, Nombre = "Acondicionamiento General", DuracionSemanas = 6, FrecuenciaSemanal = 3, Objetivo = "Mejorar resistencia cardiovascular", Nivel = "Principiante" },
                new RutinaListItem { Id = 3, Nombre = "Fuerza Funcional", DuracionSemanas = 10, FrecuenciaSemanal = 4, Objetivo = "Ganar fuerza en levantamientos básicos", Nivel = "Intermedio" },
            };

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