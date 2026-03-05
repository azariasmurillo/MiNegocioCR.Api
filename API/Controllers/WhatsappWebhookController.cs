using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;
using MiNegocioCR.Api.Infrastructure.Services;
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

        public WhatsappWebhookController(IWhatsappMessageService whatsappMessageService, IWhatsappWebhookLogRepository whatsappWebhookLogRepository)
        {
            _whatsappMessageService = whatsappMessageService;
            _whatsappWebhookLogRepository = whatsappWebhookLogRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Receive(
            [FromBody] JsonElement body,
            CancellationToken cancellationToken)
        {
            await _whatsappWebhookLogRepository.SaveAsync(body.ToString());

            await _whatsappMessageService.ProcessWebhookAsync(body);

            return Ok();
        }


    }
}
