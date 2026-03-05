using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.Business;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Security;

namespace MiNegocioCR.Api.Aplication.UseCases.Whatsapp
{
    public class WhatsappApplicationService : IWhatsappApplicationService
    {
        private readonly IWhatsappService _whatsappService;
        private readonly IAppDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly IWhatsappMessageRepository _whatsappMessageRepository;
        private readonly IGetBusinessByIdUseCase _getBusinessByIdUseCase;

        public WhatsappApplicationService(            
            IWhatsappService whatsappService,
            IAppDbContext context,
            IEncryptionService encryptionService,
            IWhatsappMessageRepository whatsappMessageRepository,
            IGetBusinessByIdUseCase _getBusinessByIdUseCase)
        {            
            _whatsappService = whatsappService;
            _context = context;
            _encryptionService = encryptionService;
            _whatsappMessageRepository = whatsappMessageRepository;
            this._getBusinessByIdUseCase = _getBusinessByIdUseCase;
        }

        public async Task SendAsync(Guid businessId, string phone, string message)
        {
            var business = await _getBusinessByIdUseCase.Execute(businessId);

            if (business == null)
                throw new Exception("Business not found");

            if (!business.EnableWhatsappNotifications)
                throw new Exception("Whatsapp not enabled for this business");

            await _whatsappService.SendAsync(business, phone, message);

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
        }

        public async Task ConnectAsync(Guid businessId, string phoneNumberId, string accessToken, CancellationToken cancellationToken = default)
        {
            var business = await _getBusinessByIdUseCase.Execute(businessId);            

            if (business == null)
                throw new Exception("Business not found");
                        
            var isValid = await _whatsappService.ValidateAsync(phoneNumberId, accessToken);

            if (!isValid)
                throw new Exception("Invalid WhatsApp credentials");

            business.WhatsappPhoneNumberId = phoneNumberId;
            business.WhatsappAccessToken = _encryptionService.Encrypt(accessToken); 
            business.EnableWhatsappNotifications = true;
            business.WhatsappTokenExpiresAt = DateTime.UtcNow.AddMonths(2);

            await _context.SaveChangesAsync(cancellationToken);
        }        
    }
}
