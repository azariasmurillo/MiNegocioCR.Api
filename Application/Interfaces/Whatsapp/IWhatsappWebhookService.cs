using System.Text.Json;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappWebhookService
    {
        Task ProcessAsync(JsonElement payload, CancellationToken cancellationToken);
    }
}
