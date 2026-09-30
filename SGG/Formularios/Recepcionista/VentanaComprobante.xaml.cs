using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
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

        public VentanaComprobante(Pago pago, string socioNombre, string socioDni, string concepto)
        {
            InitializeComponent();

            _pago = pago;
            _socioNombre = socioNombre;
            _socioDni = socioDni;
            _concepto = concepto;

            txtNumero.Text = _pago.Id.ToString();
            txtFecha.Text = _pago.FechaPago.ToString("dd/MM/yyyy HH:mm", EsAr);
            txtSocio.Text = _socioNombre;
            txtDni.Text = _socioDni;
            txtConcepto.Text = _concepto;
            txtMonto.Text = $"${_pago.Monto.ToString("N2", EsAr)}";
            txtMetodo.Text = _pago.MetodoPago;
        }

        private void btnImprimir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialogo = new PrintDialog();
                if (dialogo.ShowDialog() != true)
                    return;

                var documento = ConstruirFlowDocument();
                dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator,
                                      $"Comprobante de Pago N° {_pago.Id}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo generar el comprobante: " + ex.Message,
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        }
    }
}
