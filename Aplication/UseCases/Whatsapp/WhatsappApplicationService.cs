using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.Business;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;
using MiNegocioCR.Api.Infrastructure.Security;

namespace MiNegocioCR.Api.Aplication.UseCases.Whatsapp
{
    public class WhatsappApplicationService : IWhatsappApplicationService
    {
        private readonly IGetBusinessByIdUseCase _businessByIdUseCase;
        private readonly IWhatsappService _whatsappService;
        private readonly IAppDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public WhatsappApplicationService(
            IGetBusinessByIdUseCase businessByIdUseCase,
            IWhatsappService whatsappService,
            IAppDbContext context,
            IEncryptionService encryptionService)
        {
            _businessByIdUseCase = businessByIdUseCase;
            _whatsappService = whatsappService;
            _context = context;
            _encryptionService = encryptionService;
        }

        public async Task SendAsync(Guid businessId, string phone, string message)
        {
            var business = await _businessByIdUseCase.Execute(businessId);

            if (business == null)
                throw new Exception("Business not found");

            if (!business.EnableWhatsappNotifications)
                throw new Exception("Whatsapp not enabled for this business");

            await _whatsappService.SendAsync(business, phone, message);
        }

        public async Task ConnectAsync(Guid businessId, string phoneNumberId, string accessToken, CancellationToken cancellationToken = default)
        {
            var business = await _context.Businesses
                .FirstOrDefaultAsync(x => x.Id == businessId);

            if (business == null)
                throw new Exception("Business not found");

            // Validar token antes de guardar
            var isValid = await _whatsappService.ValidateAsync(phoneNumberId, accessToken);

            if (!isValid)
                throw new Exception("Invalid WhatsApp credentials");

            business.WhatsappPhoneNumberId = phoneNumberId;
            business.WhatsappAccessToken = _encryptionService.Encrypt(accessToken); 
            business.EnableWhatsappNotifications = true;
            business.WhatsappTokenExpiresAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }        
    }
}
