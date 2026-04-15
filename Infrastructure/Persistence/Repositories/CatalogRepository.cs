using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        private readonly AppDbContext _context;

        public CatalogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CatalogItem?> GetItemByIdAsync(Guid id)
        {
            return await _context.CatalogItems
                .AsNoTracking()
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<CatalogItem?> GetItemAsync(Guid id, Guid businessId)
        {
            return await _context.CatalogItems
                .Include(x => x.Category)
                .Include(x => x.Variants)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.BusinessId == businessId);
        }

        public async Task<List<CatalogItem>> GetItemsAsync(Guid businessId, bool includeInactive = false)
        {
            var query = _context.CatalogItems
                .AsNoTracking()
                .Include(x => x.Category)
                .Where(x => x.BusinessId == businessId);

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            return await query.ToListAsync();
        }

        public async Task AddItemAsync(CatalogItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            await _context.CatalogItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CatalogItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            _context.CatalogItems.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsWithVariantsAsync(Guid itemId)
        {
            return await _context.CatalogVariants
                .AsNoTracking()
                .AnyAsync(v => v.CatalogItemId == itemId);
        }
    }
}
