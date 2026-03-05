using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class VariantRepository : IVariantRepository
    {
        private readonly AppDbContext _context;

        public VariantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CatalogVariant?> GetVariantAsync(Guid variantId, Guid businessId)
        {
            return await _context.CatalogVariants
                .Where(v => v.Id == variantId)
                .Where(v => v.CatalogItem.BusinessId == businessId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<CatalogVariant>> GetVariantsByItemAsync(Guid catalogItemId)
        {
            return await _context.CatalogVariants
                .Where(x => x.CatalogItemId == catalogItemId)
                .ToListAsync();
        }

        public async Task AddVariantAsync(CatalogVariant variant)
        {
            await _context.CatalogVariants.AddAsync(variant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVariantAsync(CatalogVariant variant)
        {
            _context.CatalogVariants.Update(variant);
            await _context.SaveChangesAsync();
        }        
    }
}
