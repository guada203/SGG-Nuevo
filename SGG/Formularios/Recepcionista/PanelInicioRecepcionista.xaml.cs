using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SGG;
using SGG.Dominio.Entidades;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Recepcionista
{
    public partial class PanelInicioRecepcionista : UserControl
    {
        private readonly ServicioDashboard _servicioDashboard = new();
        private readonly ServicioPagos _servicioPagos = new();
        private readonly ServicioAsistencias _servicioAsistencias = new();

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
            int cuotasPorVencer = _servicioDashboard.ObtenerCantidadMembresiasPorVencer();

            txtKpiSociosActivos.Text = _servicioDashboard.ObtenerCantidadSociosActivos().ToString();
            txtKpiIngresosMes.Text = _servicioDashboard.ObtenerIngresosDelMes().ToString("C0", new CultureInfo("es-AR"));
            txtKpiCuotasPorVencer.Text = cuotasPorVencer.ToString();
            txtTendenciaCuotas.Text = $"▲ {cuotasPorVencer} próx. 30 días";
        }

        private void CargarGraficoIngresos()
        {
            // Serie de ingresos de los últimos 6 meses calculada a partir de los
            // pagos reales (Mes = primer día de cada mes, en orden ascendente).
            var ingresos = _servicioDashboard.ObtenerIngresosUltimosMeses(6);

            if (ingresos.Count == 0)
            {
                txtTendenciaIngresos.Text = "● Sin datos";
                return;
            }

            decimal mesActual = ingresos[^1].Total;
            decimal mesAnterior = ingresos.Count >= 2 ? ingresos[^2].Total : 0;

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

            decimal maxTotal = ingresos.Max(i => i.Total);
            string[] mesesCortos = { "ene", "feb", "mar", "abr", "may", "jun", "jul", "ago", "sep", "oct", "nov", "dic" };

            for (int i = 0; i < ingresos.Count; i++)
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
                    Text = FormatearMonto(ingresos[i].Total),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                Grid.SetRow(txtValor, 0);

                var barra = new Rectangle
                {
                    Width = 28,
                    Fill = new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                    RadiusX = 3,
                    RadiusY = 3,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                barra.Height = maxTotal > 0
                    ? Math.Max(6.0, (double)ingresos[i].Total / (double)maxTotal * 120.0)
                    : 6.0;
                Grid.SetRow(barra, 1);

                var mes = ingresos[i].Mes;
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
            var eventos = new List<(DateTime Fecha, string Icono, string Texto)>();

            foreach (var pago in _servicioPagos.ObtenerTodos())
            {
                eventos.Add((
                    pago.FechaPago,
                    "✅",
                    $"Nuevo pago — {NombreDeSocio(pago.Socio)}"));
            }

            foreach (var asistencia in _servicioAsistencias.ObtenerTodos())
            {
                eventos.Add((
                    asistencia.FechaHoraIngreso,
                    "🏋️",
                    $"Ingreso de socio — {NombreDeSocio(asistencia.Socio)}"));
            }

            var recientes = eventos
                .OrderByDescending(e => e.Fecha)
                .Take(5)
                .ToList();

            if (recientes.Count == 0)
            {
                spActividad.Children.Add(new TextBlock
                {
                    Text = "Todavía no hay actividad registrada.",
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                    Margin = new Thickness(0, 7, 0, 7)
                });
                return;
            }

            for (int i = 0; i < recientes.Count; i++)
            {
                var fila = new Grid();
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                fila.Margin = new Thickness(0, 7, 0, 7);

                var icono = new TextBlock
                {
                    Text = recientes[i].Icono,
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(icono, 0);

                var texto = new TextBlock
                {
                    Text = recientes[i].Texto,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(0xDD, 0xDD, 0xDD)),
                    Margin = new Thickness(10, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(texto, 1);

                var tiempo = new TextBlock
                {
                    Text = FormatearTiempo(recientes[i].Fecha),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88)),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(tiempo, 2);

                fila.Children.Add(icono);
                fila.Children.Add(texto);
                fila.Children.Add(tiempo);
                spActividad.Children.Add(fila);

                if (i < recientes.Count - 1)
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

        private static string NombreDeSocio(Socio? socio) =>
            socio == null ? "socio desconocido" : $"{socio.Nombre} {socio.Apellido}".Trim();

        /// <summary>Tiempo relativo simple para la lista de actividad reciente.</summary>
        private static string FormatearTiempo(DateTime fecha)
        {
            var transcurrido = DateTime.Now - fecha;

            if (transcurrido < TimeSpan.Zero)
                return "ahora";

            if (transcurrido.TotalMinutes < 1)
                return "hace instantes";

            if (transcurrido.TotalMinutes < 60)
                return $"hace {(int)transcurrido.TotalMinutes} min";

            if (transcurrido.TotalHours < 24)
                return $"hace {(int)transcurrido.TotalHours} h";

            return $"hace {(int)transcurrido.TotalDays} d";
        }
    }
}