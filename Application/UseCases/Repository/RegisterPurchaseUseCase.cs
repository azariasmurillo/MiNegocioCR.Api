using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class RegisterPurchaseUseCase
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IVariantRepository _variantRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public RegisterPurchaseUseCase(
            IPurchaseRepository purchaseRepository,
            IVariantRepository variantRepository,
            IInventoryRepository inventoryRepository)
        {
            _purchaseRepository = purchaseRepository;
            _variantRepository = variantRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task ExecuteAsync(
            Guid businessId,
            Guid variantId,
            int quantity,
            decimal cost)
        {
            var variant = await _variantRepository.GetVariantAsync(variantId, businessId);

            if (variant == null)
                throw new Exception("Variant not found");

            variant.StockQuantity += quantity;

            await _variantRepository.UpdateVariantAsync(variant);

            var movement = new InventoryMovement
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CatalogVariantId = variantId,
                Quantity = quantity,
                Type = InventoryMovementType.Purchase,
                CreatedAt = DateTime.UtcNow
            };

            await _inventoryRepository.AddMovementAsync(movement);
        }
    }
}
