using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddMovementAsync(InventoryMovement movement)
        {
            await _context.InventoryMovements.AddAsync(movement);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InventoryMovement>> GetMovementsAsync(Guid businessId)
        {
            return await _context.InventoryMovements
                .Where(x => x.BusinessId == businessId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<InventoryMovement>> GetMovementsByVariantAsync(Guid variantId)
        {
            return await _context.InventoryMovements
                .Where(x => x.CatalogVariantId == variantId)
                .ToListAsync();
        }
    }
}
