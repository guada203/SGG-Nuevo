using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using SGG.Dominio.Entidades;

namespace SGG.Formularios.Recepcionista
{
    /// <summary>
    /// Comprobante de pago del socio. Recibe el Pago YA guardado en la base
    /// (con su Id real) y permite imprimirlo o guardarlo como PDF eligiendo
    /// "Microsoft Print to PDF" en el diálogo de impresión de Windows.
    /// </summary>
    public partial class VentanaComprobante : Window
    {
        private static readonly CultureInfo EsAr = new("es-AR");

        private readonly Pago _pago;
        private readonly string _socioNombre;
        private readonly string _socioDni;
        private readonly string _concepto;
        private readonly DateTime? _nuevoVencimiento;

        public VentanaComprobante(Pago pago, string socioNombre, string socioDni, string concepto,
                                  DateTime? nuevoVencimiento = null)
        {
            InitializeComponent();

            _pago = pago;
            _socioNombre = socioNombre;
            _socioDni = socioDni;
            _concepto = concepto;
            _nuevoVencimiento = nuevoVencimiento;

            txtNumero.Text = _pago.Id.ToString();
            txtFecha.Text = _pago.FechaPago.ToString("dd/MM/yyyy HH:mm", EsAr);
            txtSocio.Text = _socioNombre;
            txtDni.Text = _socioDni;
            txtConcepto.Text = _concepto;
            txtMonto.Text = $"${_pago.Monto.ToString("N2", EsAr)}";
            txtMetodo.Text = _pago.MetodoPago;

            // La fila de vencimiento solo se muestra cuando el socio tiene membresía renovada.
            if (_nuevoVencimiento.HasValue)
            {
                txtVencimiento.Text = _nuevoVencimiento.Value.ToString("dd/MM/yyyy", EsAr);
                filaVencimiento.Visibility = Visibility.Visible;
            }
            else
            {
                filaVencimiento.Visibility = Visibility.Collapsed;
            }
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialogo = new PrintDialog();
                if (dialogo.ShowDialog() != true)
                    return;

                // Generar un ticket en blanco y negro solo para la impresión/PDF.
                // La ventana del sistema mantiene su estilo oscuro original.
                var ticket = ConstruirTicketParaImprimir();

                // Contenedor del tamaño de la hoja imprimible, para centrar el ticket.
                var pagina = new Grid
                {
                    Width = dialogo.PrintableAreaWidth,
                    Height = dialogo.PrintableAreaHeight,
                    Background = Brushes.White
                };
                ticket.HorizontalAlignment = HorizontalAlignment.Center;
                ticket.VerticalAlignment = VerticalAlignment.Center;
                pagina.Children.Add(ticket);

                pagina.Measure(new Size(pagina.Width, pagina.Height));
                pagina.Arrange(new Rect(new Point(0, 0), pagina.DesiredSize));
                pagina.UpdateLayout();

                dialogo.PrintVisual(pagina, $"Comprobante de Pago N° {_pago.Id}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo generar el comprobante: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border ConstruirTicketParaImprimir()
        {
            var negro = Brushes.Black;
            var gris = new SolidColorBrush(Color.FromRgb(80, 80, 80));
            var linea = new SolidColorBrush(Color.FromRgb(180, 180, 180));

            var stack = new StackPanel();

            stack.Children.Add(CrearTexto("SGG", 22, FontWeights.Bold, negro, TextAlignment.Center));
            stack.Children.Add(CrearTexto("COMPROBANTE DE PAGO", 12, FontWeights.SemiBold, negro, TextAlignment.Center, new Thickness(0, 2, 0, 0)));
            stack.Children.Add(CrearTexto("Sistema de Gestion de Gimnasio", 10, FontWeights.Normal, gris, TextAlignment.Center, new Thickness(0, 2, 0, 10)));
            stack.Children.Add(new Rectangle { Height = 1, Fill = linea, Margin = new Thickness(0, 0, 0, 10) });

            stack.Children.Add(CrearFilaTicket("Comprobante N°", _pago.Id.ToString(), negro, gris, true));
            stack.Children.Add(CrearFilaTicket("Fecha y hora", _pago.FechaPago.ToString("dd/MM/yyyy HH:mm", EsAr), negro, gris));
            stack.Children.Add(new Rectangle { Height = 1, Fill = linea, Margin = new Thickness(0, 4, 0, 8) });
            stack.Children.Add(CrearFilaTicket("Socio", _socioNombre, negro, gris));
            stack.Children.Add(CrearFilaTicket("DNI", _socioDni, negro, gris));
            stack.Children.Add(CrearFilaTicket("Concepto", _concepto, negro, gris));
            stack.Children.Add(CrearFilaTicket("Metodo de pago", _pago.MetodoPago, negro, gris));
            if (_nuevoVencimiento.HasValue)
                stack.Children.Add(CrearFilaTicket("Vencimiento", _nuevoVencimiento.Value.ToString("dd/MM/yyyy", EsAr), negro, gris, true));

            stack.Children.Add(new Rectangle { Height = 1, Fill = linea, Margin = new Thickness(0, 6, 0, 10) });

            var filaMonto = new Grid { Margin = new Thickness(0, 0, 0, 12) };
            filaMonto.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            filaMonto.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var lblMonto = CrearTexto("Monto", 13, FontWeights.SemiBold, negro, TextAlignment.Left);
            var valMonto = CrearTexto($"${_pago.Monto.ToString("N2", EsAr)}", 18, FontWeights.Bold, negro, TextAlignment.Right);
            Grid.SetColumn(lblMonto, 0);
            Grid.SetColumn(valMonto, 1);
            filaMonto.Children.Add(lblMonto);
            filaMonto.Children.Add(valMonto);
            stack.Children.Add(filaMonto);

            stack.Children.Add(CrearTexto("Gracias por tu pago", 11, FontWeights.SemiBold, negro, TextAlignment.Center, new Thickness(0, 4, 0, 2)));
            stack.Children.Add(CrearTexto("SGG - Sistema de Gestion de Gimnasio", 9, FontWeights.Normal, gris, TextAlignment.Center));

            return new Border
            {
                Width = 380,
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(22),
                Child = stack
            };
        }

        private static TextBlock CrearTexto(string texto, double fontSize, FontWeight peso, Brush color,
                                            TextAlignment align, Thickness? margin = null)
        {
            return new TextBlock
            {
                Text = texto ?? string.Empty,
                FontSize = fontSize,
                FontWeight = peso,
                Foreground = color,
                TextAlignment = align,
                TextWrapping = TextWrapping.Wrap,
                Margin = margin ?? new Thickness(0)
            };
        }

        private static Grid CrearFilaTicket(string etiqueta, string valor, Brush colorValor, Brush colorEtiqueta,
                                            bool valorNegrita = false)
        {
            var fila = new Grid { Margin = new Thickness(0, 0, 0, 6) };
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var lbl = CrearTexto(etiqueta, 11, FontWeights.Normal, colorEtiqueta, TextAlignment.Left);
            var val = CrearTexto(valor ?? string.Empty, 11, valorNegrita ? FontWeights.SemiBold : FontWeights.Normal,
                                 colorValor, TextAlignment.Right);
            Grid.SetColumn(lbl, 0);
            Grid.SetColumn(val, 1);
            fila.Children.Add(lbl);
            fila.Children.Add(val);
            return fila;
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            // Solo se asigna DialogResult si la ventana se abrió modalmente (ShowDialog),
            // igual que en el resto de los diálogos del proyecto.
            if (DialogResult.HasValue)
                DialogResult = true;

            Close();
        }

        // Genera un FlowDocument con el comprobante y lo envía al diálogo de impresión,
        // para que el usuario pueda elegir "Microsoft Print to PDF" (PDF sin librerías externas).
        private FlowDocument ConstruirFlowDocument()
        {
            var documento = new FlowDocument
            {
                PagePadding = new Thickness(40),
                FontFamily = new FontFamily("Segoe UI")
            };

            documento.Blocks.Add(new Paragraph(new Run("SGG - COMPROBANTE DE PAGO"))
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold
            });
            documento.Blocks.Add(new Paragraph(new Run("Sistema de Gestión de Gimnasio"))
            {
                FontSize = 11,
                Foreground = Brushes.Gray
            });
            documento.Blocks.Add(new Paragraph(
                    new Run($"Generado el {DateTime.Now.ToString("dd/MM/yyyy HH:mm", EsAr)}"))
            {
                FontSize = 11,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 16)
            });

            var tabla = new Table { CellSpacing = 0 };
            tabla.Columns.Add(new TableColumn { Width = new GridLength(170) });
            tabla.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            var grupo = new TableRowGroup();
            foreach (var (etiqueta, valor) in ObtenerDatosComprobante())
            {
                var fila = new TableRow();
                fila.Cells.Add(ConstruirCelda(etiqueta, true));
                fila.Cells.Add(ConstruirCelda(valor, false));
                grupo.Rows.Add(fila);
            }

            tabla.RowGroups.Add(grupo);
            documento.Blocks.Add(tabla);

            documento.Blocks.Add(new Paragraph(new Run("¡Gracias por tu pago!"))
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 18, 0, 0)
            });
            documento.Blocks.Add(new Paragraph(new Run("SGG - Sistema de Gestión de Gimnasio"))
            {
                FontSize = 10,
                Foreground = Brushes.Gray
            });

            return documento;
        }

        private static TableCell ConstruirCelda(string texto, bool negrita)
        {
            var celda = new TableCell(new Paragraph(new Run(texto)
            {
                FontWeight = negrita ? FontWeights.Bold : FontWeights.Normal
            }));
            celda.BorderBrush = Brushes.Gray;
            celda.BorderThickness = new Thickness(0.5);
            celda.Padding = new Thickness(6);
            return celda;
        }

        // Mismos pares etiqueta / valor que muestra la ventana, en el mismo orden.
        private IEnumerable<(string Etiqueta, string Valor)> ObtenerDatosComprobante()
        {
            yield return ("Comprobante N°", _pago.Id.ToString());
            yield return ("Fecha y hora", _pago.FechaPago.ToString("dd/MM/yyyy HH:mm", EsAr));
            yield return ("Socio", _socioNombre);
            yield return ("DNI", _socioDni);
            yield return ("Concepto", _concepto);
            yield return ("Monto", $"${_pago.Monto.ToString("N2", EsAr)}");
            yield return ("Método de pago", _pago.MetodoPago);
            if (_nuevoVencimiento.HasValue)
                yield return ("Vencimiento", _nuevoVencimiento.Value.ToString("dd/MM/yyyy", EsAr));
        }
    }
}
