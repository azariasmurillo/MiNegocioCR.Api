using System.Text.Json;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappMessageService
    {
        Task ProcessWebhookAsync(JsonElement payload);
    }
}
