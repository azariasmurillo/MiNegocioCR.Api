using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.AI.Intent
{
    public class IntentClassifier
    {
        public AIIntent Classify(string message)
        {
            var text = message.ToLower();

            if (text.Contains("repar"))
                return AIIntent.RepairOrder;

            if (text.Contains("compr") || text.Contains("quiero"))
                return AIIntent.Sales;

            return AIIntent.Inventory;
        }
    }
}
