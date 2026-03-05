using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Aplication.DTOs;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;
using MiNegocioCR.Api.Infrastructure.Persistence.Repositories;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsappController : ControllerBase
    {
        private readonly IWhatsappApplicationService _whatsappAppService;
        private readonly IWhatsappMessageRepository _whatsAppRepository;


        public WhatsappController(IWhatsappApplicationService whatsappAppService, IWhatsappMessageRepository repository)
        {
            _whatsappAppService = whatsappAppService;
            _whatsAppRepository = repository;
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

        [HttpGet("conversations/{businessId}")]
        public async Task<IActionResult> GetConversations(Guid businessId)
        {
            var conversations = await _whatsAppRepository
                .GetConversationsAsync(businessId);

            return Ok(conversations);
        }

        [HttpGet("messages/{businessId}/{phone}")]
        public async Task<IActionResult> GetMessages(Guid businessId, string phone)
        {
            var messages = await _whatsAppRepository
                .GetMessagesAsync(businessId, phone);

            return Ok(messages);
        }
    }
}
