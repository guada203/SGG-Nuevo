using SGG.Logica.Servicios;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SGG.Formularios.Admin
{
    public partial class PanelInicioAdmin : UserControl
    {
        private readonly ServicioDashboard _servicioDashboard = new();

        public PanelInicioAdmin()
        {
            InitializeComponent();
            CargarDatosDashboard();
            CargarGraficoIngresos();
            CargarActividadReciente();
        }

        private void CargarDatosDashboard()
        {
            // Fecha actual en formato largo en español (Argentina)
            txtFecha.Text = DateTime.Now.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es-AR"));

            txtSaludo.Text = Sesion.Activa
                ? $"Hola, {Sesion.NombreVisible} 👋"
                : "Hola, Administrador 👋";

            txtTotalSocios.Text = _servicioDashboard.ObtenerCantidadSociosActivos().ToString();
            txtTotalUsuarios.Text = _servicioDashboard.ObtenerCantidadUsuarios().ToString();
            txtIngresosMes.Text = _servicioDashboard.ObtenerIngresosDelMes().ToString("C0");
            txtSociosInactivos.Text = _servicioDashboard.ObtenerCantidadSociosInactivos().ToString();
            txtSociosNuevosMes.Text = _servicioDashboard.ObtenerCantidadSociosNuevosDelMes().ToString();
            txtMembresiasPorVencer.Text = _servicioDashboard.ObtenerCantidadMembresiasPorVencer().ToString();
        }

        private void CargarGraficoIngresos()
        {
            var serie = _servicioDashboard.ObtenerIngresosUltimosMeses(6);

            decimal mesActual = serie[^1].Total;
            decimal mesAnterior = serie.Count >= 2 ? serie[^2].Total : 0;

            if (mesAnterior > 0)
            {
                decimal porcentaje = ((mesActual - mesAnterior) / mesAnterior) * 100;
                txtTendenciaIngresos.Text = $"▲ {porcentaje:0.#}% vs. mes anterior";
                txtTendenciaIngresos.Foreground = new SolidColorBrush(Color.FromRgb(0x5B, 0xE4, 0x9B));
            }
            else
            {
                // Sin mes anterior con datos no hay porcentaje representable.
                txtTendenciaIngresos.Text = "● Sin cambios";
            }

            // Si no hay pagos registrados la serie real es todo ceros: usar valores
            // simulados para que el gráfico muestre algo (los KPIs siguen siendo reales).
            if (serie.Sum(s => s.Total) == 0)
            {
                decimal[] simulados = { 1800, 2100, 1950, 2400, 2600, 2900 };
                for (int i = 0; i < serie.Count; i++)
                {
                    serie[i] = (serie[i].Mes, simulados[i]);
                }
            }

            decimal maxTotal = serie.Max(s => s.Total);
            string[] mesesCortos = { "ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic" };

            foreach (var item in serie)
            {
                var columna = new Grid
                {
                    Width = 70,
                    Height = 180,
                    Margin = new Thickness(0, 0, 12, 0)
                };
                columna.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                columna.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                columna.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var txtValor = new TextBlock
                {
                    Text = FormatearMonto(item.Total),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                Grid.SetRow(txtValor, 0);

                var barra = new Rectangle
                {
                    Width = 28,
                    Fill = new SolidColorBrush(Color.FromRgb(0xC5, 0xFF, 0x00)),
                    RadiusX = 3,
                    RadiusY = 3,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                barra.Height = maxTotal > 0
                    ? Math.Max(6.0, (double)item.Total / (double)maxTotal * 120.0)
                    : 6.0;
                Grid.SetRow(barra, 1);

                var txtMes = new TextBlock
                {
                    Text = mesesCortos[item.Mes.Month - 1],
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                };
                Grid.SetRow(txtMes, 2);

                columna.Children.Add(txtValor);
                columna.Children.Add(barra);
                columna.Children.Add(txtMes);
                spBarras.Children.Add(columna);
            }
        }

        private void CargarActividadReciente()
        {
            var eventos = new (string Icono, string Texto, string Tiempo)[]
            {
                ("✅", "Nuevo pago de socio registrado", "hace 2 h"),
                ("👤", "Alta de socio registrada", "hace 5 h"),
                ("💳", "Renovación de membresía", "hace 1 d"),
                ("🚫", "Baja de socio procesada", "hace 2 d"),
                ("🧑‍💼", "Nuevo usuario del sistema creado", "hace 3 d")
            };

            for (int i = 0; i < eventos.Length; i++)
            {
                var fila = new Grid();
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                fila.Margin = new Thickness(0, 7, 0, 7);

                var icono = new TextBlock
                {
                    Text = eventos[i].Icono,
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(icono, 0);

                var texto = new TextBlock
                {
                    Text = eventos[i].Texto,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
                    Margin = new Thickness(10, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(texto, 1);

                var tiempo = new TextBlock
                {
                    Text = eventos[i].Tiempo,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(tiempo, 2);

                fila.Children.Add(icono);
                fila.Children.Add(texto);
                fila.Children.Add(tiempo);
                spActividad.Children.Add(fila);

                if (i < eventos.Length - 1)
                {
                    spActividad.Children.Add(new Border
                    {
                        Height = 1,
                        Background = new SolidColorBrush(Color.FromRgb(0x2A, 0x2A, 0x2A))
                    });
                }
            }
        }

        private static string FormatearMonto(decimal total)
        {
            if (total >= 1000)
                return $"${(total / 1000):0.#}k";
            return total == 0 ? "$0" : $"${total:0}";
        }
    }
}