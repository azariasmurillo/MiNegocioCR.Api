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

        public async Task SendAsync(Guid businessId, string phone, string message)
        {
            _logger.LogInformation("[SendAsync] Inicio. BusinessId: {BusinessId}, Phone: {Phone}, MessageLen: {Len}", businessId, phone, message?.Length ?? 0);

            var business = await _getBusinessByIdUseCase.Execute(businessId);
            _logger.LogDebug("[SendAsync] Business obtenido: {Found}", business != null);

            if (business == null)
                throw new NotFoundException("Business", "Business not found");

            if (!business.EnableWhatsappNotifications)
                throw new Exception("Whatsapp not enabled for this business");

            // Renovar token si está próximo a expirar (menos de 5 días)
            if (business.WhatsappTokenExpiresAt.HasValue &&
                business.WhatsappTokenExpiresAt.Value < DateTime.UtcNow.AddDays(5))
            {
                _logger.LogInformation("[SendAsync] Token próximo a expirar, intentando refresh. ExpiresAt: {ExpiresAt}", business.WhatsappTokenExpiresAt);
                var businessEntity = await _businessRepository.GetByIdAsync(businessId);
                if (businessEntity != null && !string.IsNullOrEmpty(businessEntity.WhatsappAccessToken))
                {
                    await _whatsAppTokenService.RefreshTokenAsync(businessEntity);
                    business = await _getBusinessByIdUseCase.Execute(businessId);
                    _logger.LogDebug("[SendAsync] Token refrescado, business recargado.");
                }
            }

            try
            {
                _logger.LogDebug("[SendAsync] Llamando a WhatsappService.SendAsync.");
                await _whatsappService.SendAsync(business, phone, message);
                _logger.LogDebug("[SendAsync] WhatsappService.SendAsync completado OK.");
            }
            catch (Exception ex) when (IsTokenExpiredError(ex))
            {
                _logger.LogWarning("[SendAsync] Error 190/token expirado, reintentando tras refresh. Ex: {Message}", ex.Message);
                // Error 190: token expirado → renovar y reintentar una vez
                var businessEntity = await _businessRepository.GetByIdAsync(businessId);
                if (businessEntity != null && !string.IsNullOrEmpty(businessEntity.WhatsappAccessToken))
                {
                    await _whatsAppTokenService.RefreshTokenAsync(businessEntity);
                    business = await _getBusinessByIdUseCase.Execute(businessId);
                    await _whatsappService.SendAsync(business!, phone, message);
                    _logger.LogInformation("[SendAsync] Reintento tras refresh OK.");
                }
                else
                    throw;
            }

            _logger.LogDebug("[SendAsync] Guardando WhatsAppMessage y actualizando conversación.");
            var entity = new WhatsAppMessage
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                PhoneNumber = phone,
                From = business.WhatsappPhoneNumberId!,
                To = phone,
                Body = message,
                Timestamp = DateTime.UtcNow,
                Direction = MessageDirection.Outbound,
                Status = MessageStatus.Sent
            };

            await _whatsappMessageRepository.SaveAsync(entity);
            await _whatsappMessageRepository.UpdateConversationAsync( businessId,phone,message);
            _logger.LogInformation("[SendAsync] Fin OK. Mensaje guardado y conversación actualizada.");
        }

        public async Task ConnectAsync(Guid businessId, string phoneNumberId, string accessToken, CancellationToken cancellationToken = default)
        {
            var business = await _getBusinessByIdUseCase.Execute(businessId);            

            if (business == null)
                throw new NotFoundException("Business", "Business not found");

            var isValid = await _whatsappService.ValidateAsync(phoneNumberId, accessToken);

            if (!isValid)
                throw new Exception("Invalid WhatsApp credentials");

            business.WhatsappPhoneNumberId = phoneNumberId;
            business.WhatsappAccessToken = _encryptionService.Encrypt(accessToken); 
            business.EnableWhatsappNotifications = true;
            business.WhatsappTokenExpiresAt = DateTime.UtcNow.AddMonths(2);

            await _context.SaveChangesAsync(cancellationToken);
        }

        private static bool IsTokenExpiredError(Exception ex)
        {
            var msg = ex.Message ?? "";
            return msg.Contains("190") || msg.Contains("Error validating access token", StringComparison.OrdinalIgnoreCase);
        }
    }
}
