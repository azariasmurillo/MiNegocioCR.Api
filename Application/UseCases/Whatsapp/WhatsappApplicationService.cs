using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
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

            EnsureWhatsappAccessTokenNotExpired(business);

            await _whatsappService.SendAsync(business, phone, message, attachmentUrl, attachmentType);

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

            // fb_exchange_token: short-lived user token → long-lived + real expires_in (UtcNow + seconds).
            // Si el exchange falla (p. ej. System User), guardar el token tal cual — sin error.
            var exchange = await _whatsAppTokenService.ExchangeUserTokenAsync(accessToken, cancellationToken);

            entity.WhatsappPhoneNumberId = phoneNumberId;
            entity.EnableWhatsappNotifications = true;

            if (exchange.Succeeded && !string.IsNullOrEmpty(exchange.LongLivedAccessToken))
            {
                entity.WhatsappAccessToken = _encryptionService.Encrypt(exchange.LongLivedAccessToken);
                entity.WhatsappTokenExpiresAt = exchange.ExpiresAtUtc;
                _logger.LogInformation(
                    "[Connect WhatsApp] Long-lived token stored for business {BusinessId}. ExpiresInSeconds: {ExpiresIn}, ExpiresUtc: {Expires}",
                    businessId, exchange.ExpiresInSeconds, exchange.ExpiresAtUtc);
            }
            else
            {
                _logger.LogWarning(
                    "[Connect WhatsApp] Exchange not applied (System User, expired, missing AppId/Secret, etc.); storing token as provided. BusinessId: {BusinessId}. AppCredentialsMissing: {Missing}, SessionExpired: {Expired}, Meta: {Meta}",
                    businessId, exchange.AppCredentialsMissing, exchange.SessionExpired, TokenLogMask.TruncateForLog(exchange.ErrorBody));
                entity.WhatsappAccessToken = _encryptionService.Encrypt(accessToken);
                entity.WhatsappTokenExpiresAt = null;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// User long-lived tokens store <see cref="GetBusinessByIdResultDto.WhatsappTokenExpiresAt"/> from Meta <c>expires_in</c>.
        /// System User tokens leave expiry null — no DB-based expiry check.
        /// </summary>
        private static void EnsureWhatsappAccessTokenNotExpired(GetBusinessByIdResultDto business)
        {
            if (business.WhatsappTokenExpiresAt is not { } expiresAtUtc)
                return;

            if (DateTime.UtcNow >= expiresAtUtc)
                throw new UnauthorizedAccessException(WhatsappReconnectRequired.Code);
        }
    }
}
