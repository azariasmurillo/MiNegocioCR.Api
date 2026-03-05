using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly AppDbContext _context;

        public SaleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddSaleAsync(Sale sale)
        {
            if (sale == null)
                throw new ArgumentNullException(nameof(sale));
            await _context.Sales.AddAsync(sale);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Sale>> GetSalesAsync(Guid businessId)
        {
            return await _context.Sales
                .Where(x => x.BusinessId == businessId)
                .ToListAsync();
        }

        public async Task<Sale?> GetSaleAsync(Guid id, Guid businessId)
        {
            return await _context.Sales
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.BusinessId == businessId);
        }
    }
}
