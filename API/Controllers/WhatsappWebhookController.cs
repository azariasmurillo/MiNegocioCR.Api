using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp/webhook")]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IWhatsappWebhookService _webhookService;
        private const string VerifyToken = "MiNegocioCR_Webhook_Secret";

        public WhatsappWebhookController(IWhatsappWebhookService webhookService)
        {
            _webhookService = webhookService;
        }

        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.challenge")] string challenge,
            [FromQuery(Name = "hub.verify_token")] string verifyToken)
        {
            if (mode == "subscribe" && verifyToken == VerifyToken)
            {
                return Ok(challenge);
            }

            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] JsonElement payload, CancellationToken cancellationToken)
        {
            await _webhookService.ProcessAsync(payload, cancellationToken);
            return Ok();
        }
    }
}
