using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.ArchiveConversation;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Application.Interfaces.ConversationTag;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;

namespace MiNegocioCR.Api.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsappController : ControllerBase
    {
        private readonly IWhatsappApplicationService _whatsappAppService;
        private readonly IWhatsappMessageRepository _whatsAppRepository;
        private readonly IMarkConversationReadHandler _markConversationReadHandler;
        private readonly ICreateConversationHandler _createConversationHandler;
        private readonly IUpdateConversationStatusHandler _updateConversationStatusHandler;
        private readonly ISendTemplateHandler _sendTemplateHandler;
        private readonly IConversationTag _conversationTag;
        private readonly IContact _contact;
        private readonly IGetUnreadTotalUseCase _getUnreadTotalUseCase;
        private readonly IArchiveConversationUseCase _archiveConversationUseCase;

        public WhatsappController(IWhatsappApplicationService whatsappAppService,
            IWhatsappMessageRepository repository,
            IMarkConversationReadHandler markConversationReadHandler,
            ICreateConversationHandler createConversationHandler,
            IUpdateConversationStatusHandler updateConversationStatusHandler,
            ISendTemplateHandler sendTemplateHandler,
            IConversationTag conversationTag,
            IContact contact,
            IGetUnreadTotalUseCase getUnreadTotalUseCase,
            IArchiveConversationUseCase archiveConversationUseCase)
        {
            _whatsappAppService = whatsappAppService;
            _whatsAppRepository = repository;
            _markConversationReadHandler = markConversationReadHandler;
            _createConversationHandler = createConversationHandler;
            _updateConversationStatusHandler = updateConversationStatusHandler;
            _sendTemplateHandler = sendTemplateHandler;
            _conversationTag = conversationTag;
            _contact = contact;
            _getUnreadTotalUseCase = getUnreadTotalUseCase;
            _archiveConversationUseCase = archiveConversationUseCase;
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendWhatsappRequestDto request)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            await _whatsappAppService.SendByConversationIdAsync(
                request.BusinessId,
                request.ConversationId,
                request.Message ?? "",
                request.AttachmentUrl,
                request.AttachmentType);

            return Ok();
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect([FromBody] ConnectWhatsappRequestDto request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest("Request body is required.");

            await _whatsappAppService.ConnectAsync(
                request.BusinessId,
                request.PhoneNumberId,
                request.AccessToken,
                cancellationToken);

            return Ok(new { message = "WhatsApp connected successfully" });
        }

        [HttpGet("conversations/{businessId}")]
        public async Task<IActionResult> GetConversations(Guid businessId
            , [FromQuery] string? search
            , [FromQuery] string? phone
            , [FromQuery] string? name
            , [FromQuery] string? tag)
        {
            var conversations = await _whatsAppRepository
                .GetConversationsAsync(businessId, search, phone, name, tag);

            return Ok(conversations);
        }

        [HttpGet("messages/{businessId}/{conversationId:guid}")]
        public async Task<IActionResult> GetMessages(Guid businessId, Guid conversationId, [FromQuery] int limit = 50)
        {
            var messages = await _whatsAppRepository.GetMessagesAsync(businessId, conversationId, limit);

            return Ok(messages);
        }

        [HttpPatch("conversations/{conversationId:guid}/read")]
        public async Task<IActionResult> MarkRead(Guid conversationId, [FromBody] MarkConversationReadCommandDto command)
        {
            if (command == null)
                return BadRequest("Request body is required.");
            command.ConversationId = conversationId;
            await _markConversationReadHandler.Handle(command);
            return Ok();
        }

        [HttpPost("conversation")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationCommandDto command)
        {
            await _createConversationHandler.Handle(command);

            return Ok();
        }

        [HttpPatch("conversation/status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateConversationStatusCommandDto command)
        {
            await _updateConversationStatusHandler.Handle(command);

            return Ok();
        }

        [HttpPost("send-template")]
        public async Task<IActionResult> SendTemplate([FromBody] SendTemplateCommandDto command)
        {
            await _sendTemplateHandler.Handle(command);

            return Ok();
        }

        /// <summary>Lista de etiquetas usadas en el negocio (filtros / UI).</summary>
        [HttpGet("business/{businessId:guid}/conversation-tags")]
        public async Task<IActionResult> GetDistinctConversationTags(Guid businessId)
        {
            var tags = await _conversationTag.GetDistinctTagsForBusinessAsync(businessId);
            return Ok(tags);
        }

        [HttpPost("conversations/{conversationId:guid}/tags")]
        public async Task<IActionResult> AddTag(
            Guid conversationId,
            [FromBody] SetConversationTagRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Tag))
                return BadRequest("Body with BusinessId and Tag is required.");
            await _conversationTag.AddTagAsync(request.BusinessId, conversationId, request.Tag);
            return Ok();
        }

        [HttpDelete("conversations/{conversationId:guid}/tags/{tag}")]
        public async Task<IActionResult> RemoveTag(
            Guid conversationId,
            string tag,
            [FromQuery] Guid businessId)
        {
            await _conversationTag.RemoveTagAsync(businessId, conversationId, tag);
            return Ok();
        }

        [HttpGet("conversations/{conversationId:guid}/tags")]
        public async Task<IActionResult> GetTags(Guid conversationId, [FromQuery] Guid businessId)
        {
            var tags = await _conversationTag.GetTagsAsync(businessId, conversationId);
            return Ok(tags);
        }

        [HttpPost("contacts/import")]
        public async Task<IActionResult> ImportContacts(
            [FromBody] ImportContactsRequestDto request)
        {
            await _contact.ImportContactsAsync(
                request.BusinessId,
                request.Contacts);

            return Ok();
        }

        [HttpGet("contacts/{businessId}")]
        public async Task<IActionResult> GetContacts(Guid businessId)
        {
            var contacts = await _contact.GetContactsAsync(businessId);

            return Ok(contacts);
        }

        [HttpGet("conversations/{businessId}/unread-total")]
        public async Task<IActionResult> GetUnreadTotal(Guid businessId)
        {
            var total = await _getUnreadTotalUseCase.Execute(businessId);

            return Ok(total);
        }

        [HttpPatch("conversations/{conversationId:guid}/archive")]
        public async Task<IActionResult> ArchiveConversation(
            Guid conversationId,
            [FromBody] ArchiveConversationDto request)
        {
            await _archiveConversationUseCase.Execute(
                conversationId,
                request.IsArchived);

            return Ok();
        }
    }
}
