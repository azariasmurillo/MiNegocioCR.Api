using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using System.Text.Json;
using System.Threading;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/webhook/whatsapp")]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IWhatsappMessageService _whatsappMessageService;
        private readonly IWhatsappWebhookLogRepository _whatsappWebhookLogRepository;
        private readonly ILogger<WhatsappWebhookController> _logger;

        public WhatsappWebhookController(
            IWhatsappMessageService whatsappMessageService,
            IWhatsappWebhookLogRepository whatsappWebhookLogRepository,
            ILogger<WhatsappWebhookController> logger)
        {
            _whatsappMessageService = whatsappMessageService;
            _whatsappWebhookLogRepository = whatsappWebhookLogRepository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Receive(
            [FromBody] JsonElement body,
            CancellationToken cancellationToken)
        {
            var hasEntry = body.TryGetProperty("entry", out _);
            _logger.LogInformation(
                "[WhatsApp Webhook] POST recibido. Tiene entry: {HasEntry}, payload length: {Length}",
                hasEntry, body.GetRawText().Length);

            await _whatsappWebhookLogRepository.SaveAsync(body.ToString());

            await _whatsappMessageService.ProcessWebhookAsync(body);

            return Ok();
        }
    }
}
