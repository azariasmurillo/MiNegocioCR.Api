using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Number;

namespace MiNegocioCR.Api.Application.AI.Parsing
{
    /// <summary>
    /// Extrae una cantidad numérica desde un mensaje de texto (dígitos o números en español).
    /// Reutilizable desde IA, webhooks, etc.
    /// </summary>
    public static class QuantityParser
    {
        /// <summary>
        /// Intenta obtener la primera cantidad mencionada en el mensaje.
        /// Ejemplos: "3", "3 estarían bien", "con dos estaría bien", "cinco por favor" → 3, 2, 5.
        /// </summary>
        /// <param name="message">Texto del usuario (puede contener dígitos o palabras como dos, cinco).</param>
        /// <returns>Cantidad encontrada, o null si no se reconoce ninguna.</returns>
        public static int? TryParseQuantity(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var results = NumberRecognizer.RecognizeNumber(message, Culture.Spanish);

            if (!results.Any())
                return null;

            if (int.TryParse(results.First().Resolution["value"].ToString(), out var quantity))
                return quantity;

            return null;
        }
    }
}
