using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SGG
{
    /// <summary>
    /// Validación de entrada en el front. Instala handlers de tipeo y pegado
    /// para restringir qué caracteres puede escribir el usuario.
    /// </summary>
    public static class Validadores
    {
        /// <summary>Permite solo dígitos (opcionalmente coma/punto para decimales).</summary>
        public static void SoloNumeros(TextBox campo, bool permitirDecimal = false)
        {
            campo.PreviewTextInput += (s, e) =>
            {
                e.Handled = !EsTextoValido(e.Text, permitirDecimal);
            };

            DataObject.AddPastingHandler(campo, (s, e) =>
            {
                if (!e.DataObject.GetDataPresent(DataFormats.Text)) return;
                string texto = (string)e.DataObject.GetData(DataFormats.Text);
                if (!EsTextoValido(texto, permitirDecimal))
                    e.CancelCommand();
            });
        }

        /// <summary>Permite solo letras (incluye acentos y ñ), espacios y apóstrofes.</summary>
        public static void SoloLetras(TextBox campo)
        {
            campo.PreviewTextInput += (s, e) =>
            {
                e.Handled = !e.Text.All(c => char.IsLetter(c) || c == ' ' || c == '\'');
            };

            DataObject.AddPastingHandler(campo, (s, e) =>
            {
                if (!e.DataObject.GetDataPresent(DataFormats.Text)) return;
                string texto = (string)e.DataObject.GetData(DataFormats.Text);
                if (!texto.All(c => char.IsLetter(c) || c == ' ' || c == '\''))
                    e.CancelCommand();
            });
        }

        private static bool EsTextoValido(string texto, bool permitirDecimal)
        {
            return permitirDecimal
                ? texto.All(c => char.IsDigit(c) || c == ',' || c == '.')
                : texto.All(char.IsDigit);
        }
    }
}