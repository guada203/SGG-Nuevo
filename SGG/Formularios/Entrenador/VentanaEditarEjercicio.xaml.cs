using System.Windows;

namespace SGG.Formularios.Entrenador
{
    public partial class VentanaEditarEjercicio : Window
    {
        public EjercicioItem Resultado { get; private set; }

        // Constructor para agregar un ejercicio nuevo
        public VentanaEditarEjercicio()
        {
            InitializeComponent();
        }

        // Constructor para editar uno existente
        public VentanaEditarEjercicio(EjercicioItem ejercicioExistente) : this()
        {
            txtTitulo.Text = "Editar Ejercicio";
            this.Title = "Editar Ejercicio";

            txtNombre.Text = ejercicioExistente.Nombre;
            txtGrupoMuscular.Text = ejercicioExistente.GrupoMuscular;
            txtSeries.Text = ejercicioExistente.Series.ToString();
            txtRepeticiones.Text = ejercicioExistente.Repeticiones.ToString();
            txtDescanso.Text = ejercicioExistente.DescansoSegundos.ToString();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Ingresá el nombre del ejercicio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtSeries.Text, out int series) || series <= 0)
            {
                MessageBox.Show("Las series deben ser un número mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtRepeticiones.Text, out int repeticiones) || repeticiones <= 0)
            {
                MessageBox.Show("Las repeticiones deben ser un número mayor a cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDescanso.Text, out int descanso) || descanso < 0)
            {
                MessageBox.Show("El descanso debe ser un número (0 o mayor).", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Resultado = new EjercicioItem
            {
                Nombre = txtNombre.Text,
                GrupoMuscular = string.IsNullOrWhiteSpace(txtGrupoMuscular.Text) ? "General" : txtGrupoMuscular.Text,
                Series = series,
                Repeticiones = repeticiones,
                DescansoSegundos = descanso
            };

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}