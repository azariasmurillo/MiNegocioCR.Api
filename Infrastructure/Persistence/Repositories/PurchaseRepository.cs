using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class PurchaseRepository : IPurchaseRepository
    {
        private readonly AppDbContext _context;

        public PurchaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddPurchaseAsync(Purchase purchase)
        {
            if (purchase == null)
                throw new ArgumentNullException(nameof(purchase));
            await _context.Purchases.AddAsync(purchase);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Purchase>> GetPurchasesAsync(Guid businessId)
        {
            return await _context.Purchases
                .Where(x => x.BusinessId == businessId)
                .ToListAsync();
        }

        public async Task<Purchase?> GetPurchaseAsync(Guid id, Guid businessId)
        {
            return await _context.Purchases
                .FirstOrDefaultAsync(x => x.Id == id && x.BusinessId == businessId);
        }
    }
}
