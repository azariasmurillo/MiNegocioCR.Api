using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/webhook")]
    public class WhatsappWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;

        public WhatsappWebhookController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Verify(
            [FromQuery(Name = "hub.mode")] string mode,
            [FromQuery(Name = "hub.verify_token")] string verifyToken,
            [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var myToken = _config["WHATSAPP_VERIFY_TOKEN"];

            if (mode == "subscribe" && verifyToken == myToken)
            {
                return Ok(challenge);
            }

            return Forbid();
        }

        [HttpPost]
        public async Task<IActionResult> Receive([FromBody] object body)
        {
            Console.WriteLine("📩 Mensaje recibido:");
            Console.WriteLine(body);

            return Ok();
        }
    }
}
