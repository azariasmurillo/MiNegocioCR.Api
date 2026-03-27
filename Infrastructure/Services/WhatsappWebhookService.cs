using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using System.Text.Json;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    /// <summary>
    /// Compatibilidad: el webhook HTTP usa <see cref="IWhatsappMessageService"/>.
    /// Este servicio delega al mismo flujo para no duplicar lógica ni crear mensajes sin conversación.
    /// </summary>
    public class WhatsappWebhookService : IWhatsappWebhookService
    {
        private readonly IWhatsappMessageService _whatsappMessageService;

        public WhatsappWebhookService(IWhatsappMessageService whatsappMessageService)
        {
            _whatsappMessageService = whatsappMessageService;
        }

        public Task ProcessAsync(JsonElement payload, CancellationToken cancellationToken) =>
            _whatsappMessageService.ProcessWebhookAsync(payload);
    }
}
