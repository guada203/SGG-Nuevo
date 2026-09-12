using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SGG;

namespace SGG.Formularios.Recepcionista
{
    public partial class PanelInicioRecepcionista : UserControl
    {
        public PanelInicioRecepcionista()
        {
            InitializeComponent();
            CargarEncabezado();
            CargarKpis();
            CargarGraficoIngresos();
            CargarActividadReciente();
        }

        private void CargarEncabezado()
        {
            // Fecha actual en formato largo en español (Argentina)
            txtFecha.Text = DateTime.Today.ToString("D", new CultureInfo("es-AR"));

            txtSaludo.Text = Sesion.Activa
                ? $"Hola, {Sesion.NombreVisible} 👋"
                : "Hola, Recepcionista 👋";
        }

        private void CargarKpis()
        {
            int cuotasPorVencer = DatosRecepDemo.CantidadCuotasPorVencer();

            txtKpiSociosActivos.Text = DatosRecepDemo.CantidadSociosActivos().ToString();
            txtKpiIngresosMes.Text = DatosRecepDemo.IngresosDelMes().ToString("C0", new CultureInfo("es-AR"));
            txtKpiCuotasPorVencer.Text = cuotasPorVencer.ToString();
            txtTendenciaCuotas.Text = $"▲ {cuotasPorVencer} próx. 30 días";
        }

        private void CargarGraficoIngresos()
        {
            decimal[] serie = DatosRecepDemo.IngresosPorMes();

            decimal mesActual = serie[^1];
            decimal mesAnterior = serie.Length >= 2 ? serie[^2] : 0;

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

            decimal maxTotal = serie.Max();
            string[] mesesCortos = { "ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic" };
            DateTime hoy = DateTime.Today;

            for (int i = 0; i < serie.Length; i++)
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
                    Text = FormatearMonto(serie[i]),
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
                    ? Math.Max(6.0, (double)serie[i] / (double)maxTotal * 120.0)
                    : 6.0;
                Grid.SetRow(barra, 1);

                // Mes correspondiente: desde hace 5 meses hasta el actual
                var mes = hoy.AddMonths(i - (serie.Length - 1));
                var txtMes = new TextBlock
                {
                    Text = mesesCortos[mes.Month - 1],
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
                ("✅", "Nuevo pago registrado — Camila Benítez", "hace 30 min"),
                ("👤", "Alta de socio — Camila Benítez", "hace 1 h"),
                ("🏋️", "Ingreso de socio — Diego Torres", "hace 2 h"),
                ("💳", "Pago con tarjeta — Lucía Fernández", "hace 1 d"),
                ("⚠️", "Cuota vencida — Ana García", "hace 2 d")
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