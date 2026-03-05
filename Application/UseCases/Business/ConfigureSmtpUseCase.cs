using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Business
{
    public class ConfigureSmtpUseCase : IConfigureSmtpUseCase
    {
        private readonly IAppDbContext _context;

        public ConfigureSmtpUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task Execute(Guid businessId, ConfigureSmtpDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var business = await _context.Businesses.FindAsync(businessId);

            if (business == null)
                throw new NotFoundException("Business", "Business not found");

            business.SmtpHost = dto.SmtpHost;
            business.SmtpPort = dto.SmtpPort;
            business.SmtpUsername = dto.SmtpUsername;
            business.SmtpPassword = dto.SmtpPassword;
            business.SmtpFromEmail = dto.SmtpFromEmail;
            business.SmtpFromName = dto.SmtpFromName;

            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}
