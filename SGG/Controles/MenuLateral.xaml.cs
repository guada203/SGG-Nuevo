using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SGG.Controles
{
    public partial class MenuLateral : UserControl
    {
        // Evento que la ventana que lo use va a "escuchar"
        public event Action<string>? OpcionSeleccionada;

        // DependencyProperty para indicar qué botón debe estar resaltado según la ventana activa
        public static readonly DependencyProperty SeccionActivaProperty =
            DependencyProperty.Register(nameof(SeccionActiva), typeof(string), typeof(MenuLateral), new PropertyMetadata("Inicio", OnSeccionActivaChanged));

        public string SeccionActiva
        {
            get => (string)GetValue(SeccionActivaProperty);
            set => SetValue(SeccionActivaProperty, value);
        }

        private static void OnSeccionActivaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MenuLateral menuLateral)
            {
                menuLateral.MarcarActivo((string)e.NewValue);
            }
        }

        public MenuLateral()
        {
            InitializeComponent();
        }

        // Configurar el rol muestra el panel correspondiente
        public void ConfigurarRol(string rol)
        {
            // Normalizar entrada: evitar espacios y diferencias de mayúsculas
            string rolNorm = (rol ?? string.Empty).Trim();
            txtRol.Text = rolNorm.ToUpper();

            panelAdmin.Visibility = string.Equals(rolNorm, "Administrador", System.StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible : Visibility.Collapsed;
            panelRecepcionista.Visibility = string.Equals(rolNorm, "Recepcionista", System.StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible : Visibility.Collapsed;
            panelEntrenador.Visibility = string.Equals(rolNorm, "Entrenador", System.StringComparison.OrdinalIgnoreCase)
                ? Visibility.Visible : Visibility.Collapsed;

            // Aplicar el botón activo por defecto (según SeccionActiva)
            MarcarActivo(SeccionActiva);
        }

        // Resalta el botón cuyo Tag coincida con el parámetro; resto quedan Transparent
        private void MarcarActivo(string tagBotonActivo)
        {
            foreach (var panel in new[] { panelAdmin, panelRecepcionista, panelEntrenador })
            {
                foreach (var child in panel.Children)
                {
                    if (child is Button boton)
                    {
                        // Si es el botón activo, ponlo en #252525; si no, Transparent
                        if (boton.Tag?.ToString() == tagBotonActivo)
                        {
                            boton.Background = (System.Windows.Media.Brush)new BrushConverter().ConvertFrom("#252525")!;
                        }
                        else
                        {
                            boton.Background = System.Windows.Media.Brushes.Transparent;
                        }
                    }
                }
            }
        }

        private void Navegar_Click(object sender, RoutedEventArgs e)
        {
            var boton = (Button)sender;
            string opcion = boton.Tag?.ToString() ?? "";
            OpcionSeleccionada?.Invoke(opcion);
        }
    }
}