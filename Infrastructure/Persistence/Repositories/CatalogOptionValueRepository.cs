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

        public async Task<List<CatalogOptionValue>> GetByCatalogOptionIdAsync(Guid catalogOptionId)
        {
            return await _context.CatalogOptionValues
                .AsNoTracking()
                .Where(x => x.CatalogOptionId == catalogOptionId)
                .OrderBy(x => x.Value)
                .ToListAsync();
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
    }
}
