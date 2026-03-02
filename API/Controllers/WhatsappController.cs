using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Aplication.DTOs;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsappController : ControllerBase
    {
        private readonly IWhatsappApplicationService _whatsappAppService;

        public WhatsappController(IWhatsappApplicationService whatsappAppService)
        {
            _whatsappAppService = whatsappAppService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendWhatsappRequestDto request)
        {
            await _whatsappAppService.SendAsync(
                request.BusinessId,
                request.Phone,
                request.Message);

            return Ok();
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect([FromBody] ConnectWhatsappRequestDto request, CancellationToken cancellationToken)
        {
            await _whatsappAppService.ConnectAsync(
                request.BusinessId,
                request.PhoneNumberId,
                request.AccessToken,
                cancellationToken);

            return Ok(new { message = "WhatsApp connected successfully" });
        }
    }
}
