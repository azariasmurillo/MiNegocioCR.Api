using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly AppDbContext _context;

        public BusinessRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Business?> GetByWhatsappPhoneNumberIdAsync(string phoneNumberId)
        {
            return await _context.Businesses
                .FirstOrDefaultAsync(x => x.WhatsappPhoneNumberId == phoneNumberId);
        }
    }
}
