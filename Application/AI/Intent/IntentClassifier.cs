using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.AI.Intent
{
    public class IntentClassifier : IIntentClassifier
    {

        public AIIntent Classify(string message)
        {
            var text = message.ToLower().Trim();

            var statusKeywords = new[]
            {
                "como esta",
                "como está",
                "como va",
                "estado",
                "mi laptop",
                "mi computadora",
                "mi equipo",
                "mi reparación",
                "mi reparacion",
                "ya esta",
                "ya está"
            };

            if (statusKeywords.Any(k => text.Contains(k)))
            {
                return AIIntent.RepairOrder;
            }

            var repairWords = new[]
            {
                "repar",
                "arregl",
                "dañ",
                "jodi",
                "quebr",
                "pantalla",
                "no enciende",
                "no prende",
                "no carga",
                "no funciona",
                "problema",
                "mal"
            };

            if (repairWords.Any(w => text.Contains(w)))
                return AIIntent.RepairService;


            if (text.Contains("compr") || text.Contains("quiero"))
                return AIIntent.Sales;

            return AIIntent.Inventory;
        }
    }
}
