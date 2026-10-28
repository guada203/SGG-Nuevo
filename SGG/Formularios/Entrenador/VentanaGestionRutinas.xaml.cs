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
using SGG.Logica.Servicios;

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

        private readonly ServicioSocios _servicioSocios = new();
        private readonly ServicioRutinas _servicioRutinas = new();

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

            // Buscamos la rutina completa en SQL Server.
            var rutinaDb = _servicioRutinas.ObtenerPorId(_rutinaIdEnEdicion);

            if (rutinaDb == null)
            {
                MessageBox.Show(
                    "No se encontró la rutina.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            // Cargamos las notas guardadas.
            txtNotas.Text = rutinaDb.Notas;

            // Limpiamos la lista antes de cargar los ejercicios.
            Ejercicios.Clear();

            // Convertimos los ejercicios de SQL Server
            // en elementos que puede mostrar nuestra ventana.
            foreach (var ejercicio in rutinaDb.Ejercicios)
            {
                Ejercicios.Add(new EjercicioItem
                {
                    Nombre = ejercicio.Nombre,
                    GrupoMuscular = ejercicio.GrupoMuscular,
                    Series = ejercicio.Series,
                    Repeticiones = ejercicio.Repeticiones,
                    DescansoSegundos = ejercicio.DescansoSegundos
                });
            }

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

            // Buscamos la rutina original con todos sus ejercicios.
            var rutinaOriginal = _servicioRutinas.ObtenerPorId(plantilla.Id);

            if (rutinaOriginal != null)
            {
                // Copiamos las notas de la rutina original.
                txtNotas.Text = rutinaOriginal.Notas;

                // Cargamos copias de los ejercicios.
                Ejercicios.Clear();

                foreach (var ejercicio in rutinaOriginal.Ejercicios)
                {
                    Ejercicios.Add(new EjercicioItem
                    {
                        Nombre = ejercicio.Nombre,
                        GrupoMuscular = ejercicio.GrupoMuscular,
                        Series = ejercicio.Series,
                        Repeticiones = ejercicio.Repeticiones,
                        DescansoSegundos = ejercicio.DescansoSegundos
                    });
                }
            }

            txtBreadcrumb.Text = "Mis Alumnos  >  Nueva Rutina";
            txtTitulo.Text = $"Nueva Rutina (basada en \"{plantilla.Nombre}\")";
        }

        private void InicializarDatos()
        {
            icEjercicios.ItemsSource = Ejercicios;

            _sociosTodos = _servicioSocios.ObtenerTodos()
                .Where(s => s.Activo && s.EntrenadorId == Sesion.UsuarioId)
                .Select(s => new SocioComboItem
                {
                    Id = s.Id,
                    NombreCompleto = $"{s.Nombre} {s.Apellido} - DNI {s.Dni}"
                })
                .ToList();
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
            // 1. Comprobamos que el formulario tenga los datos necesarios.
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresá el nombre de la rutina.");
                return;
            }

            if (cmbSocios.SelectedItem is not SocioComboItem socioSeleccionado)
            {
                MessageBox.Show("Seleccioná un socio de la lista.");
                return;
            }
            bool sigueAsignado = _servicioSocios.ObtenerTodos()
                .Any(s => s.Id == socioSeleccionado.Id
           && s.Activo
           && s.EntrenadorId == Sesion.UsuarioId);

            if (!sigueAsignado)
            {
                MessageBox.Show(
                    "Este socio ya no está asignado a tu entrenador. No se puede guardar la rutina.",
                    "Socio no disponible",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtSemanas.Text, out int semanas) || semanas <= 0)
            {
                MessageBox.Show("La duración debe ser mayor a cero.");
                return;
            }

            if (Ejercicios.Count == 0)
            {
                MessageBox.Show("Agregá al menos un ejercicio.");
                return;
            }

            // 2. Obtenemos los valores seleccionados en el formulario.
            int frecuencia = cmbFrecuencia.SelectedIndex switch
            {
                0 => 2,
                1 => 3,
                2 => 4,
                3 => 5,
                _ => 3
            };

            string nivel = rbAvanzado.IsChecked == true ? "Avanzado"
                         : rbIntermedio.IsChecked == true ? "Intermedio"
                         : "Principiante";

            string estado = rbCompletada.IsChecked == true
                ? "Completada"
                : "Activa";

            // 3. Creamos un objeto Rutina con los datos del formulario.
            var rutina = new Rutina
            {
                Id = _modoEdicion ? _rutinaIdEnEdicion : 0,
                SocioId = socioSeleccionado.Id,
                Nombre = txtNombre.Text.Trim(),
                Objetivo = txtObjetivo.Text.Trim(),
                DuracionSemanas = semanas,
                FrecuenciaSemanal = frecuencia,
                Nivel = nivel,
                Estado = estado,
                Notas = txtNotas.Text.Trim(),

                // Convertimos los ejercicios de la ventana
                // en entidades que Entity Framework puede guardar.
                Ejercicios = Ejercicios.Select(ejercicio => new Ejercicio
                {
                    Nombre = ejercicio.Nombre,
                    GrupoMuscular = ejercicio.GrupoMuscular,
                    Series = ejercicio.Series,
                    Repeticiones = ejercicio.Repeticiones,
                    DescansoSegundos = ejercicio.DescansoSegundos
                }).ToList()
            };

            // 4. Pedimos al servicio que cree o modifique la rutina.
            try
            {
                var resultado = _modoEdicion
                    ? _servicioRutinas.ModificarRutina(rutina)
                    : _servicioRutinas.CrearRutina(rutina);

                if (!resultado.Exitoso)
                {
                    MessageBox.Show(
                        resultado.Mensaje,
                        "Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // 5. Cerramos la ventana solamente si se guardó bien.
                MessageBox.Show(
                    resultado.Mensaje,
                    "Éxito",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"No se pudo guardar la rutina.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
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