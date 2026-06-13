using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class CatalogOptionValueRepository : ICatalogOptionValueRepository
    {
        private readonly AppDbContext _context;

        public CatalogOptionValueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CatalogOptionValue optionValue)
        {
            if (optionValue == null)
                throw new ArgumentNullException(nameof(optionValue));

            await _context.CatalogOptionValues.AddAsync(optionValue);
            await _context.SaveChangesAsync();
        }

        public async Task<CatalogOptionValue?> GetByIdAsync(Guid id)
        {
            return await _context.CatalogOptionValues
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<CatalogOptionValue>> GetByCatalogOptionIdAsync(Guid catalogOptionId, bool includeInactive = false)
        {
            var query = _context.CatalogOptionValues
                .AsNoTracking()
                .Where(x => x.CatalogOptionId == catalogOptionId);

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            return await query
                .OrderBy(x => x.Value)
                .ToListAsync();
        }

        public async Task<bool> ExistsActiveValueAsync(
            Guid catalogOptionId,
            string normalizedValue,
            Guid? excludeValueId = null)
        {
            var key = normalizedValue.Trim().ToLowerInvariant();
            var query = _context.CatalogOptionValues
                .AsNoTracking()
                .Where(x =>
                    x.CatalogOptionId == catalogOptionId &&
                    x.IsActive &&
                    x.Value.ToLower() == key);

            if (excludeValueId.HasValue)
            {
                query = query.Where(x => x.Id != excludeValueId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<List<CatalogOptionValue>> GetByIdsWithCatalogOptionAsync(IReadOnlyList<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return new List<CatalogOptionValue>();

            return await _context.CatalogOptionValues
                .AsNoTracking()
                .Include(v => v.CatalogOption)
                .Where(v => ids.Contains(v.Id))
                .ToListAsync();
        }

        public async Task UpdateAsync(CatalogOptionValue optionValue)
        {
            if (optionValue == null)
                throw new ArgumentNullException(nameof(optionValue));

            _context.CatalogOptionValues.Update(optionValue);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsInVariantsAsync(Guid optionValueId)
        {
            return await _context.CatalogVariantOptionValues
                .AsNoTracking()
                .AnyAsync(x => x.CatalogOptionValueId == optionValueId);
        }

        public async Task DeleteAsync(CatalogOptionValue optionValue)
        {
            if (optionValue == null)
                throw new ArgumentNullException(nameof(optionValue));

            _context.CatalogOptionValues.Remove(optionValue);
            await _context.SaveChangesAsync();
        }
    }
}
