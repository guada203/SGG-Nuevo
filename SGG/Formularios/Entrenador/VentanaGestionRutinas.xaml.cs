using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using SGG.Dominio.Entidades;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaGestionRutinas : Window
    {
        public ObservableCollection<EjercicioItem> Ejercicios { get; set; } = new ObservableCollection<EjercicioItem>();

        private bool _modoEdicion = false;
        private int _rutinaIdEnEdicion = 0;

        public VentanaGestionRutinas()
        {
            InitializeComponent();
            InicializarDatos();
        }

        public VentanaGestionRutinas(int socioIdPrecargado) : this()
        {
            var socios = (List<SocioComboItem>)cmbSocios.ItemsSource;
            cmbSocios.SelectedItem = socios.FirstOrDefault(s => s.Id == socioIdPrecargado);
        }

        public VentanaGestionRutinas(RutinaListItem rutinaAEditar) : this()
        {
            _modoEdicion = true;
            _rutinaIdEnEdicion = rutinaAEditar.Id;

            txtNombre.Text = rutinaAEditar.Nombre;
            txtObjetivo.Text = rutinaAEditar.Objetivo;
            txtSemanas.Text = rutinaAEditar.DuracionSemanas.ToString();

            var socios = (List<SocioComboItem>)cmbSocios.ItemsSource;
            cmbSocios.SelectedItem = socios.FirstOrDefault(s => s.Id == rutinaAEditar.SocioId);

            switch (rutinaAEditar.FrecuenciaSemanal)
            {
                case 2: cmbFrecuencia.SelectedIndex = 0; break;
                case 3: cmbFrecuencia.SelectedIndex = 1; break;
                case 4: cmbFrecuencia.SelectedIndex = 2; break;
                case 5: cmbFrecuencia.SelectedIndex = 3; break;
                default: cmbFrecuencia.SelectedIndex = 1; break;
            }

            switch (rutinaAEditar.Nivel)
            {
                case "Principiante": rbPrincipiante.IsChecked = true; break;
                case "Intermedio": rbIntermedio.IsChecked = true; break;
                case "Avanzado": rbAvanzado.IsChecked = true; break;
            }

            txtBreadcrumb.Text = "Mis Rutinas  >  Editar Rutina";
            txtTitulo.Text = "Editar Rutina";
            btnGuardarRutina.Content = "Guardar Cambios";
            this.Title = "SGG - Editar Rutina";
        }

        private void InicializarDatos()
        {
            Ejercicios.Add(new EjercicioItem { Nombre = "Press de Banca", GrupoMuscular = "Pecho", Series = 4, Repeticiones = 12, DescansoSegundos = 90 });
            Ejercicios.Add(new EjercicioItem { Nombre = "Sentadilla con Barra", GrupoMuscular = "Piernas", Series = 4, Repeticiones = 10, DescansoSegundos = 120 });
            Ejercicios.Add(new EjercicioItem { Nombre = "Peso Muerto", GrupoMuscular = "Espalda", Series = 3, Repeticiones = 8, DescansoSegundos = 90 });
            Ejercicios.Add(new EjercicioItem { Nombre = "Dominadas", GrupoMuscular = "Espalda", Series = 3, Repeticiones = 10, DescansoSegundos = 60 });

            icEjercicios.ItemsSource = Ejercicios;

            cmbSocios.ItemsSource = new List<SocioComboItem>
            {
                new SocioComboItem { Id = 1, NombreCompleto = "Juan Pérez - DNI 40123456" },
                new SocioComboItem { Id = 2, NombreCompleto = "María Gómez - DNI 38654987" },
                new SocioComboItem { Id = 3, NombreCompleto = "Lucas Rodríguez - DNI 42987123" }
            };
        }

        private void btnGuardarRutina_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Por favor, ingresá el nombre de la rutina.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbSocios.SelectedValue == null)
            {
                MessageBox.Show("Por favor, seleccioná un socio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string nombreRutina = txtNombre.Text;
            string mensaje = _modoEdicion
                ? $"Rutina '{nombreRutina}' actualizada correctamente (simulado)."
                : $"Rutina '{nombreRutina}' creada correctamente con {Ejercicios.Count} ejercicios (simulado).";

            MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnEliminarEjercicio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elemento && elemento.DataContext is EjercicioItem ejercicio)
            {
                Ejercicios.Remove(ejercicio);
            }
        }

        private void btnAgregarEjercicio_Click(object sender, RoutedEventArgs e)
        {
            Ejercicios.Add(new EjercicioItem
            {
                Nombre = "Nuevo Ejercicio",
                GrupoMuscular = "General",
                Series = 3,
                Repeticiones = 12,
                DescansoSegundos = 60
            });
        }
    }

    public class EjercicioItem
    {
        public string Nombre { get; set; } = string.Empty;
        public string GrupoMuscular { get; set; } = string.Empty;
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public int DescansoSegundos { get; set; }
    }

    public class SocioComboItem
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;

        public override string ToString()
        {
            return NombreCompleto;
        }
    }
}