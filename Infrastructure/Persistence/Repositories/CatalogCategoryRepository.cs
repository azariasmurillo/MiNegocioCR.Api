using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class CatalogCategoryRepository : ICatalogCategoryRepository
    {
        private readonly AppDbContext _context;

        public CatalogCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(CatalogCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            await _context.CatalogCategories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task<CatalogCategory?> GetByIdAsync(Guid id)
        {
            return await _context.CatalogCategories
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<CatalogCategory>> GetByBusinessIdAsync(Guid businessId, bool includeInactive = false)
        {
            var query = _context.CatalogCategories
                .AsNoTracking()
                .Where(x => x.BusinessId == businessId);

            if (!includeInactive)
                query = query.Where(x => x.IsActive);

            return await query
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task UpdateAsync(CatalogCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            _context.CatalogCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsWithProductsAsync(Guid categoryId)
        {
            return await _context.CatalogItems
                .AsNoTracking()
                .AnyAsync(x => x.CategoryId == categoryId);
        }
    }
}
