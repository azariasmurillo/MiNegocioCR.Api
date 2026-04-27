using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IInventoryRepository
    {
        Task AddMovementAsync(InventoryMovement movement);

        Task<List<InventoryMovement>> GetMovementsAsync(Guid businessId);

        Task<List<InventoryMovement>> GetMovementsByVariantAsync(Guid businessId, Guid variantId);
    }
}
