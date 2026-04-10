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
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<CatalogOption>> GetByCatalogItemIdAsync(Guid catalogItemId)
        {
            return await _context.CatalogOptions
                .AsNoTracking()
                .Where(x => x.CatalogItemId == catalogItemId)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}
