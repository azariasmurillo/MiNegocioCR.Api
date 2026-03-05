using System.Text.Json;

namespace MiNegocioCR.Api.Aplication.Interfaces.Whatsapp
{
    public interface IWhatsappMessageService
    {
        Task ProcessWebhookAsync(JsonElement payload);
    }
}
