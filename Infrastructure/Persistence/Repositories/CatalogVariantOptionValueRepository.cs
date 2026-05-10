using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class CatalogVariantOptionValueRepository : ICatalogVariantOptionValueRepository
    {
        private readonly AppDbContext _context;

        public CatalogVariantOptionValueRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CatalogVariantOptionValue link)
        {
            if (link == null)
                throw new ArgumentNullException(nameof(link));

            await _context.CatalogVariantOptionValues.AddAsync(link);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IReadOnlyList<CatalogVariantOptionValue> links)
        {
            if (links == null)
                throw new ArgumentNullException(nameof(links));
            if (links.Count == 0)
                return;

            await _context.CatalogVariantOptionValues.AddRangeAsync(links);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsVariantWithSameOptionValueCombinationAsync(
            Guid catalogItemId,
            IReadOnlyList<Guid> sortedDistinctOptionValueIds)
        {
            if (sortedDistinctOptionValueIds == null)
                return false;

            if (sortedDistinctOptionValueIds.Count == 0)
            {
                return await _context.CatalogVariants
                    .AsNoTracking()
                    .Where(v => v.CatalogItemId == catalogItemId)
                    .Where(v => !_context.CatalogVariantOptionValues.Any(l => l.CatalogVariantId == v.Id))
                    .AnyAsync();
            }

            var variantIds = await _context.CatalogVariants
                .AsNoTracking()
                .Where(v => v.CatalogItemId == catalogItemId)
                .Select(v => v.Id)
                .ToListAsync();

            if (variantIds.Count == 0)
                return false;

            var links = await _context.CatalogVariantOptionValues
                .AsNoTracking()
                .Where(l => variantIds.Contains(l.CatalogVariantId))
                .Select(l => new { l.CatalogVariantId, l.CatalogOptionValueId })
                .ToListAsync();

            var target = sortedDistinctOptionValueIds.ToList();
            var expectedCount = target.Count;

            foreach (var group in links.GroupBy(x => x.CatalogVariantId))
            {
                var valueIds = group.Select(x => x.CatalogOptionValueId).OrderBy(x => x).ToList();
                if (valueIds.Count != expectedCount)
                    continue;
                if (valueIds.SequenceEqual(target))
                    return true;
            }

            return false;
        }
    }
}
