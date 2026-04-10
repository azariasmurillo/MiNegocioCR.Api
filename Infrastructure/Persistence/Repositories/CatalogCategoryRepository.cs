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

        public async Task<List<CatalogCategory>> GetByBusinessIdAsync(Guid businessId)
        {
            return await _context.CatalogCategories
                .AsNoTracking()
                .Where(x => x.BusinessId == businessId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}
