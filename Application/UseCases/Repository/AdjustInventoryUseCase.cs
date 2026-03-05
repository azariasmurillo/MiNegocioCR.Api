using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class AdjustInventoryUseCase
    {
        private readonly IVariantRepository _variantRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public AdjustInventoryUseCase(
            IVariantRepository variantRepository,
            IInventoryRepository inventoryRepository)
        {
            _variantRepository = variantRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task ExecuteAsync(
            Guid businessId,
            Guid variantId,
            int adjustment,
            string reason)
        {
            if (adjustment == 0)
                throw new ArgumentException("Adjustment must be a non-zero value.", nameof(adjustment));
            
            var variant = await _variantRepository.GetVariantAsync(variantId, businessId);

            if (variant == null)
                throw new NotFoundException("CatalogVariant", "Variant not found");

            variant.StockQuantity += adjustment;

            await _variantRepository.UpdateVariantAsync(variant);

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CatalogVariantId = variantId,
                Quantity = adjustment,
                Type = InventoryMovementType.Adjustment,
                Notes = reason,
                CreatedAt = DateTime.UtcNow
            };

            await _inventoryRepository.AddMovementAsync(movement);
        }
    }
}
