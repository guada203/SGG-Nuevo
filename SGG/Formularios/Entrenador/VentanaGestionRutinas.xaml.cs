using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using SGG.Dominio.Entidades;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaGestionRutinas : Window
    {
        public ObservableCollection<EjercicioItem> Ejercicios { get; set; } = new ObservableCollection<EjercicioItem>();

        private List<SocioComboItem> _sociosTodos = new List<SocioComboItem>();
        private ICollectionView _vistaSocios;
        private string _textoBusquedaSocio = "";

        private bool _modoEdicion = false;
        private int _rutinaIdEnEdicion = 0;

        public VentanaGestionRutinas()
        {
            InitializeComponent();
            Validadores.SoloNumeros(txtSemanas);
            InicializarDatos();
        }

        public VentanaGestionRutinas(int socioIdPrecargado) : this()
        {
            SeleccionarSocio(socioIdPrecargado);
        }

        public VentanaGestionRutinas(RutinaListItem rutinaAEditar) : this()
        {
            _modoEdicion = true;
            _rutinaIdEnEdicion = rutinaAEditar.Id;

            txtNombre.Text = rutinaAEditar.Nombre;
            txtObjetivo.Text = rutinaAEditar.Objetivo;
            txtSemanas.Text = rutinaAEditar.DuracionSemanas.ToString();

            SeleccionarSocio(rutinaAEditar.SocioId);
            SeleccionarFrecuencia(rutinaAEditar.FrecuenciaSemanal);
            SeleccionarNivel(rutinaAEditar.Nivel);
            SeleccionarEstado(rutinaAEditar.Estado);

            txtBreadcrumb.Text = "Mis Rutinas  >  Editar Rutina";
            txtTitulo.Text = "Editar Rutina";
            btnGuardarRutina.Content = "Guardar Cambios";
            this.Title = "SGG - Editar Rutina";
        }

        public VentanaGestionRutinas(RutinaListItem plantilla, int nuevoSocioId) : this()
        {
            txtNombre.Text = plantilla.Nombre;
            txtObjetivo.Text = plantilla.Objetivo;
            txtSemanas.Text = plantilla.DuracionSemanas.ToString();

            SeleccionarSocio(nuevoSocioId);
            SeleccionarFrecuencia(plantilla.FrecuenciaSemanal);
            SeleccionarNivel(plantilla.Nivel);

            txtBreadcrumb.Text = "Mis Alumnos  >  Nueva Rutina";
            txtTitulo.Text = $"Nueva Rutina (basada en \"{plantilla.Nombre}\")";
        }

        private void InicializarDatos()
        {
            Ejercicios.Add(new EjercicioItem { Nombre = "Press de Banca", GrupoMuscular = "Pecho", Series = 4, Repeticiones = 12, DescansoSegundos = 90 });
            Ejercicios.Add(new EjercicioItem { Nombre = "Sentadilla con Barra", GrupoMuscular = "Piernas", Series = 4, Repeticiones = 10, DescansoSegundos = 120 });
            Ejercicios.Add(new EjercicioItem { Nombre = "Peso Muerto", GrupoMuscular = "Espalda", Series = 3, Repeticiones = 8, DescansoSegundos = 90 });
            Ejercicios.Add(new EjercicioItem { Nombre = "Dominadas", GrupoMuscular = "Espalda", Series = 3, Repeticiones = 10, DescansoSegundos = 60 });

            icEjercicios.ItemsSource = Ejercicios;

            _sociosTodos = new List<SocioComboItem>
            {
                new SocioComboItem { Id = 1, NombreCompleto = "Juan Pérez - DNI 40123456" },
                new SocioComboItem { Id = 2, NombreCompleto = "María Gómez - DNI 38654987" },
                new SocioComboItem { Id = 3, NombreCompleto = "Lucas Rodríguez - DNI 42987123" }
            };

            cmbSocios.ItemsSource = _sociosTodos;

            // La "vista" es una capa sobre la lista que permite ocultar elementos
            // sin sacarlos de la lista original.
            _vistaSocios = CollectionViewSource.GetDefaultView(cmbSocios.ItemsSource);
            _vistaSocios.Filter = FiltrarSocio;

            // Escuchamos lo que se escribe dentro del propio ComboBox
            cmbSocios.AddHandler(TextBoxBase.TextChangedEvent,
                                 new TextChangedEventHandler(cmbSocios_TextChanged));
        }

        private bool FiltrarSocio(object item)
        {
            if (string.IsNullOrEmpty(_textoBusquedaSocio)) return true;

            var socio = item as SocioComboItem;
            if (socio == null) return false;

            return socio.NombreCompleto.ToLower().Contains(_textoBusquedaSocio);
        }

        private void cmbSocios_TextChanged(object sender, TextChangedEventArgs e)
        {
            var seleccionado = cmbSocios.SelectedItem as SocioComboItem;

            // Si el texto coincide exactamente con el socio ya elegido, es porque
            // alguien lo seleccionó (o lo precargamos): limpiamos el filtro y no abrimos nada.
            if (seleccionado != null && cmbSocios.Text == seleccionado.NombreCompleto)
            {
                _textoBusquedaSocio = "";
                _vistaSocios?.Refresh();
                return;
            }

            _textoBusquedaSocio = cmbSocios.Text?.Trim().ToLower() ?? "";
            _vistaSocios?.Refresh();

            if (!cmbSocios.IsDropDownOpen)
            {
                cmbSocios.IsDropDownOpen = true;
            }
        }

        private void SeleccionarSocio(int socioId)
        {
            cmbSocios.SelectedItem = _sociosTodos.FirstOrDefault(s => s.Id == socioId);
        }

        private void SeleccionarFrecuencia(int frecuencia)
        {
            switch (frecuencia)
            {
                case 2: cmbFrecuencia.SelectedIndex = 0; break;
                case 3: cmbFrecuencia.SelectedIndex = 1; break;
                case 4: cmbFrecuencia.SelectedIndex = 2; break;
                case 5: cmbFrecuencia.SelectedIndex = 3; break;
                default: cmbFrecuencia.SelectedIndex = 1; break;
            }
        }

        private void SeleccionarNivel(string nivel)
        {
            switch (nivel)
            {
                case "Principiante": rbPrincipiante.IsChecked = true; break;
                case "Intermedio": rbIntermedio.IsChecked = true; break;
                case "Avanzado": rbAvanzado.IsChecked = true; break;
            }
        }

        private void SeleccionarEstado(string estado)
        {
            switch (estado)
            {
                case "Activa": rbActiva.IsChecked = true; break;
                case "Completada": rbCompletada.IsChecked = true; break;
                default: rbActiva.IsChecked = true; break;
            }
        }

        private void btnGuardarRutina_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Por favor, ingresá el nombre de la rutina.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbSocios.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccioná un socio de la lista.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtSemanas.Text, out int semanas) || semanas <= 0)
            {
                MessageBox.Show("La duración debe ser un número mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Ejercicios.Count == 0)
            {
                MessageBox.Show("Agregá al menos un ejercicio a la rutina.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtObjetivo.Text))
            {
                MessageBox.Show("Por favor, ingresá el objetivo de la rutina.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string nombreRutina = txtNombre.Text;
            string estadoElegido = rbActiva.IsChecked == true ? "Activa" : "Completada";
            string mensaje = _modoEdicion
    ? $"Rutina '{nombreRutina}' actualizada correctamente. Estado: {estadoElegido} (simulado)."
    : $"Rutina '{nombreRutina}' creada correctamente con {Ejercicios.Count} ejercicios. Estado: {estadoElegido} (simulado).";

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
            var ventana = new VentanaEditarEjercicio();
            ventana.Owner = this;
            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                Ejercicios.Add(ventana.Resultado);
            }
        }

        private void btnEditarEjercicio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elemento && elemento.DataContext is EjercicioItem ejercicioActual)
            {
                var ventana = new VentanaEditarEjercicio(ejercicioActual);
                ventana.Owner = this;
                bool? resultado = ventana.ShowDialog();

                if (resultado == true)
                {
                    int indice = Ejercicios.IndexOf(ejercicioActual);
                    Ejercicios[indice] = ventana.Resultado;
                }
            }
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