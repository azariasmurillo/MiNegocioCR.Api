using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Whatsapp
{
    public class WhatsappApplicationService : IWhatsappApplicationService
    {
        private readonly IWhatsappService _whatsappService;
        private readonly IAppDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IWhatsappMessageRepository _whatsappMessageRepository;
        private readonly IGetBusinessByIdUseCase _getBusinessByIdUseCase;
        private readonly IWhatsAppTokenService _whatsAppTokenService;
        private readonly IBusinessRepository _businessRepository;
        private readonly ILogger<WhatsappApplicationService> _logger;

        public WhatsappApplicationService(
            IWhatsappService whatsappService,
            IAppDbContext context,
            IEncryptionService encryptionService,
            IWhatsappMessageRepository whatsappMessageRepository,
            IGetBusinessByIdUseCase getBusinessByIdUseCase,
            IWhatsAppTokenService whatsAppTokenService,
            IBusinessRepository businessRepository,
            ILogger<WhatsappApplicationService> logger)
        {
            _whatsappService = whatsappService;
            _context = context;
            _encryptionService = encryptionService;
            _whatsappMessageRepository = whatsappMessageRepository;
            _getBusinessByIdUseCase = getBusinessByIdUseCase;
            _whatsAppTokenService = whatsAppTokenService;
            _businessRepository = businessRepository;
            _logger = logger;
        }

        public async Task SendByConversationIdAsync(Guid businessId, Guid conversationId, string message,
            string? attachmentUrl = null, string? attachmentType = null)
        {
            _logger.LogInformation(
                "[SendByConversationId] BusinessId: {BusinessId}, ConversationId: {ConversationId}",
                businessId, conversationId);

            var conv = await _whatsappMessageRepository.GetConversationByIdAsync(conversationId, businessId);
            if (conv == null)
                throw new NotFoundException("WhatsAppConversation", "Conversation not found");

            var phone = conv.PhoneNumber ?? throw new InvalidOperationException("Conversation has no phone number.");

            var business = await _getBusinessByIdUseCase.Execute(businessId);
            if (business == null)
                throw new NotFoundException("Business", "Business not found");

            if (!business.EnableWhatsappNotifications)
                throw new InvalidOperationException("Whatsapp not enabled for this business");

            try
            {
                await _whatsappService.SendAsync(business, phone, message, attachmentUrl, attachmentType);
            }
            catch (Exception ex) when (IsTokenExpiredError(ex))
            {
                var businessEntity = await _businessRepository.GetByIdAsync(businessId);
                if (businessEntity != null && !string.IsNullOrEmpty(businessEntity.WhatsappAccessToken))
                {
                    await _whatsAppTokenService.RefreshTokenAsync(businessEntity);
                    business = await _getBusinessByIdUseCase.Execute(businessId);
                    await _whatsappService.SendAsync(business!, phone, message, attachmentUrl, attachmentType);
                }
                else
                    throw;
            }

            var messageId = Guid.NewGuid();
            var entity = new WhatsAppMessage
            {
                Id = messageId,
                MessageId = $"out-{messageId:N}",
                ConversationId = conversationId,
                PhoneNumber = phone,
                From = business.WhatsappPhoneNumberId!,
                To = phone,
                Body = message ?? "",
                AttachmentUrl = attachmentUrl,
                AttachmentType = attachmentType,
                Timestamp = DateTime.UtcNow,
                Direction = MessageDirection.Outbound,
                Status = MessageStatus.Sent,
                CreatedAt = DateTime.UtcNow
            };

            await _whatsappMessageRepository.SaveAsync(entity);

            await _whatsappMessageRepository.UpdateConversationAfterMessageAsync(conversationId, message ?? "",
                MessageDirection.Outbound);
        }

        public async Task SendAsync(Guid businessId, string phone, string message, string? attachmentUrl = null,
            string? attachmentType = null)
        {
            var conv = await _whatsappMessageRepository.GetOrCreateConversationAsync(businessId, phone, null);
            await SendByConversationIdAsync(businessId, conv.Id, message, attachmentUrl, attachmentType);
        }

        public async Task ConnectAsync(Guid businessId, string phoneNumberId, string accessToken,
            CancellationToken cancellationToken = default)
        {
            var entity = await _businessRepository.GetByIdAsync(businessId);

            if (entity == null)
                throw new NotFoundException("Business", "Business not found");

            var isValid = await _whatsappService.ValidateAsync(phoneNumberId, accessToken);

            if (!isValid)
                throw new InvalidOperationException("Invalid WhatsApp credentials");

            entity.WhatsappPhoneNumberId = phoneNumberId;
            entity.WhatsappAccessToken = _encryptionService.Encrypt(accessToken);
            entity.EnableWhatsappNotifications = true;
            entity.WhatsappTokenExpiresAt = DateTime.UtcNow.AddMonths(2);

            await _context.SaveChangesAsync(cancellationToken);
        }

        private static bool IsTokenExpiredError(Exception ex)
        {
            var msg = ex.Message ?? "";
            return msg.Contains("190") || msg.Contains("Error validating access token", StringComparison.OrdinalIgnoreCase);
        }
    }
}
