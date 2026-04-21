using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class CatalogOptionRepository : ICatalogOptionRepository
    {
        private readonly AppDbContext _context;

        public CatalogOptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CatalogOption option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            await _context.CatalogOptions.AddAsync(option);
            await _context.SaveChangesAsync();
        }

        public async Task<CatalogOption?> GetByIdAsync(Guid id)
        {
            return await _context.CatalogOptions
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<CatalogOption>> GetByCatalogItemIdAsync(Guid catalogItemId, bool includeInactive = false)
        {
            var query = _context.CatalogOptions
                .AsNoTracking()
                .Where(x => x.CatalogItemId == catalogItemId);

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            return await query
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task UpdateAsync(CatalogOption option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            _context.CatalogOptions.Update(option);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsWithValuesAsync(Guid optionId)
        {
            return await _context.CatalogOptionValues
                .AsNoTracking()
                .AnyAsync(x => x.CatalogOptionId == optionId);
        }

        public async Task DeleteAsync(CatalogOption option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            _context.CatalogOptions.Remove(option);
            await _context.SaveChangesAsync();
        }
    }
}
