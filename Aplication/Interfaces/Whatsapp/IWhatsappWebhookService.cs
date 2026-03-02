using System.Text.Json;

namespace MiNegocioCR.Api.Aplication.Interfaces.Whatsapp
{
    public interface IWhatsappWebhookService
    {
        Task ProcessAsync(JsonElement payload, CancellationToken cancellationToken);
    }
}
