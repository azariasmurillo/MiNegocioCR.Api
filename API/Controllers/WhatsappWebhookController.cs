using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;
using System.Text.Json;
using System.Threading;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWhatsappWebhookService _whatsappWebhookService;

        public WhatsappWebhookController(IConfiguration config, IWhatsappWebhookService whatsappWebhookService)
        {
            _config = config;
            _whatsappWebhookService = whatsappWebhookService;
        }

        [HttpGet]
        public IActionResult Verify(
        [FromQuery(Name = "hub.mode")] string mode,
        [FromQuery(Name = "hub.verify_token")] string verifyToken,
        [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var myToken = _config["WHATSAPP_VERIFY_TOKEN"];

            if (string.IsNullOrEmpty(myToken))
                return StatusCode(500, "Verify token not configured");

            if (mode == "subscribe" && verifyToken == myToken)
                return Ok(challenge);

            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] JsonElement body, CancellationToken cancellationToken )
        {
            //Tests
            Console.WriteLine("📩 RAW WEBHOOK:");
            Console.WriteLine(body.ToString());

            if (body.TryGetProperty("entry", out var entry))
            {
                var changes = entry[0].GetProperty("changes");
                var value = changes[0].GetProperty("value");

                if (value.TryGetProperty("messages", out var messages))
                {
                    var message = messages[0];

                    var from = message.GetProperty("from").GetString();
                    var text = message.GetProperty("text").GetProperty("body").GetString();

                    Console.WriteLine($"📱 From: {from}");
                    Console.WriteLine($"💬 Message: {text}");
                }
            }

            return Ok();

            //production
            //await _whatsappWebhookService.ProcessAsync(body, cancellationToken);
            //return Ok();
        }
    }
}
