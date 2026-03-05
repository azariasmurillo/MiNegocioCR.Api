using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;

namespace MiNegocioCR.Api.Application.UseCases.Business
{
    public class GetBusinessByIdUseCase : IGetBusinessByIdUseCase
    {
        private readonly IAppDbContext _context;

        public GetBusinessByIdUseCase(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<GetBusinessByIdResultDto?> Execute(Guid id)
        {
            return await _context.Businesses
                .Where(x => x.Id == id)
                .Select(x => new GetBusinessByIdResultDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    EnableEmailNotifications = x.EnableEmailNotifications,
                    EnableWhatsappNotifications = x.EnableWhatsappNotifications,
                    WhatsappPhoneNumberId = x.WhatsappPhoneNumberId,
                    WhatsappBusinessAccountId = x.WhatsappBusinessAccountId,
                    WhatsappDisplayPhoneNumber = x.WhatsappDisplayPhoneNumber,
                    WhatsappTokenExpiresAt = x.WhatsappTokenExpiresAt,
                    SmtpHost = x.SmtpHost,
                    SmtpPort = x.SmtpPort,
                    SmtpUsername = x.SmtpUsername,
                    SmtpFromEmail = x.SmtpFromEmail,
                    SmtpFromName = x.SmtpFromName,
                    CreatedAt = x.CreatedAt,
                    WhatsappAccessToken = x.WhatsappAccessToken
                })
                .FirstOrDefaultAsync();
        }
    }
}
