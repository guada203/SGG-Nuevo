using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SGG.Controles
{
    public partial class GraficoTorta : UserControl
    {
        private IReadOnlyList<GraficoTortaItem> _items = Array.Empty<GraficoTortaItem>();

        public GraficoTorta()
        {
            InitializeComponent();
            PieCanvas.SizeChanged += (_, _) => Renderizar();
        }

        /// <summary>
        /// Recibe los datos y redibuja el gráfico de torta.
        /// Cada item aporta un sector con su etiqueta, valor numérico y color hex.
        /// </summary>
        public void SetDatos(IReadOnlyList<GraficoTortaItem> items)
        {
            _items = items ?? Array.Empty<GraficoTortaItem>();
            Renderizar();
        }

        // ── Dibujo del gráfico ───────────────────────────────────────────────

        private void Renderizar()
        {
            PieCanvas.Children.Clear();
            LeyendaPanel.Children.Clear();

            if (_items.Count == 0 || PieCanvas.ActualWidth == 0 || PieCanvas.ActualHeight == 0)
                return;

            double cx = PieCanvas.ActualWidth / 2.0;
            double cy = PieCanvas.ActualHeight / 2.0;
            double radio = Math.Min(cx, cy) - 6;
            if (radio <= 0) return;

            double total = _items.Sum(i => i.Valor);
            if (total <= 0) return;

            double anguloActual = -90; // arranca desde arriba (12 en punto)

            foreach (var item in _items)
            {
                double porcentaje = item.Valor / total;
                double barrido = porcentaje * 360.0;

                if (barrido <= 0) { anguloActual += barrido; continue; }

                // Sector de 100 % → círculo completo
                if (porcentaje >= 0.9995)
                {
                    var elipse = new EllipseGeometry(new Point(cx, cy), radio, radio);
                    var caminoCirculo = new Path
                    {
                        Fill = ConvertirBrush(item.ColorHex),
                        Data = elipse
                    };
                    PieCanvas.Children.Add(caminoCirculo);

                    // Tooltip en el círculo completo
                    ToolTipService.SetToolTip(caminoCirculo, FormatearTooltip(item, porcentaje));
                }
                else
                {
                    double radInicio = anguloActual * Math.PI / 180.0;
                    double radFin = (anguloActual + barrido) * Math.PI / 180.0;

                    Point puntoInicio = new Point(cx + radio * Math.Cos(radInicio), cy + radio * Math.Sin(radInicio));
                    Point puntoFin = new Point(cx + radio * Math.Cos(radFin), cy + radio * Math.Sin(radFin));

                    bool arcoLargo = barrido > 180;

                    var figura = new PathFigure { StartPoint = new Point(cx, cy), IsClosed = true };
                    figura.Segments.Add(new LineSegment(puntoInicio, true));
                    figura.Segments.Add(new ArcSegment(
                        puntoFin,
                        new Size(radio, radio),
                        0,
                        arcoLargo,
                        SweepDirection.Clockwise,
                        true
                    ));

                    var geometria = new PathGeometry();
                    geometria.Figures.Add(figura);

                    var camino = new Path
                    {
                        Fill = ConvertirBrush(item.ColorHex),
                        Data = geometria
                    };
                    PieCanvas.Children.Add(camino);

                    ToolTipService.SetToolTip(camino, FormatearTooltip(item, porcentaje));
                }

                anguloActual += barrido;
            }

            // Leyenda
            foreach (var item in _items)
            {
                double porcentaje = (item.Valor / total) * 100.0;
                string textoPct = ((int)Math.Round(porcentaje)).ToString(CultureInfo.InvariantCulture);

                var fila = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 6) };

                // Cuadrado de color
                var swatch = new Border
                {
                    Background = ConvertirBrush(item.ColorHex),
                    Width = 12,
                    Height = 12,
                    CornerRadius = new CornerRadius(2),
                    Margin = new Thickness(0, 1, 8, 0)
                };
                fila.Children.Add(swatch);

                // Etiqueta
                var lblEtiqueta = new TextBlock
                {
                    Text = item.Etiqueta,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA")),
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };
                fila.Children.Add(lblEtiqueta);

                // Porcentaje
                var lblPct = new TextBlock
                {
                    Text = $" {textoPct}%",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };
                fila.Children.Add(lblPct);

                LeyendaPanel.Children.Add(fila);
            }
        }

        // ── Utilidades ───────────────────────────────────────────────────────

        private static string FormatearTooltip(GraficoTortaItem item, double porcentaje)
        {
            string pct = ((int)Math.Round(porcentaje * 100.0)).ToString(CultureInfo.InvariantCulture);
            return $"{item.Etiqueta}: {item.Valor.ToString("N0", CultureInfo.InvariantCulture)} ({pct}%)";
        }

        private static Brush ConvertirBrush(string hex)
        {
            try
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch
            {
                return Brushes.Gray;
            }
        }
    }

    /// <summary>
    /// Elemento de datos para el gráfico de torta.
    /// </summary>
    public class GraficoTortaItem
    {
        public string Etiqueta { get; set; } = string.Empty;
        public double Valor { get; set; }
        public string ColorHex { get; set; } = "#888888";
    }
}
