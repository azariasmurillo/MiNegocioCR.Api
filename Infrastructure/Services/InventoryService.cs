using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IVariantRepository _variantRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILowStockAlertService _alertService;

        public InventoryService(
            IVariantRepository variantRepository,
            IInventoryRepository inventoryRepository,
            ILowStockAlertService alertService)
        {
            _variantRepository = variantRepository;
            _inventoryRepository = inventoryRepository;
            _alertService = alertService;
        }

        public async Task IncreaseStockAsync(
            Guid businessId,
            Guid variantId,
            int quantity,
            string reference)
        {
            var variant = await _variantRepository.GetVariantAsync(variantId, businessId);

            if (variant == null)
                throw new Exception("Variant not found");

            if (quantity <= 0)
                throw new Exception("Quantity must be greater than zero");

            variant.StockQuantity += quantity;

            await _variantRepository.UpdateVariantAsync(variant);

            await _inventoryRepository.AddMovementAsync(new InventoryMovement
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CatalogVariantId = variantId,
                Quantity = quantity,
                Type = InventoryMovementType.Purchase,
                Reference = reference,
                CreatedAt = DateTime.UtcNow
            });
        }

        public async Task DecreaseStockAsync(
            Guid businessId,
            Guid variantId,
            int quantity,
            string reference)
        {
            var variant = await _variantRepository.GetVariantAsync(variantId, businessId);

            if (variant == null)
                throw new Exception("Variant not found");

            if (variant.StockQuantity < quantity)
                throw new Exception("Not enough stock");

            if (quantity <= 0)
                throw new Exception("Quantity must be greater than zero");

            variant.StockQuantity -= quantity;

            await _variantRepository.UpdateVariantAsync(variant);

            await _inventoryRepository.AddMovementAsync(new InventoryMovement
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CatalogVariantId = variantId,
                Quantity = -quantity,
                Type = InventoryMovementType.Sale,
                Reference = reference,
                CreatedAt = DateTime.UtcNow
            });
            if (variant.StockQuantity <= variant.LowStockThreshold)
            {
                await _alertService.NotifyLowStock(businessId, variant);
            }
        }

        public async Task AdjustStockAsync(
            Guid businessId,
            Guid variantId,
            int quantity,
            string reason)
        {
            var variant = await _variantRepository.GetVariantAsync(variantId, businessId);

            if (variant == null)
                throw new Exception("Variant not found");

            if (quantity <= 0)
                throw new Exception("Quantity must be greater than zero");

            variant.StockQuantity += quantity;

            await _variantRepository.UpdateVariantAsync(variant);

            await _inventoryRepository.AddMovementAsync(new InventoryMovement
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CatalogVariantId = variantId,
                Quantity = quantity,
                Type = InventoryMovementType.Adjustment,
                Notes = reason,
                CreatedAt = DateTime.UtcNow
            });
            if (variant.StockQuantity <= variant.LowStockThreshold)
            {
                await _alertService.NotifyLowStock(businessId, variant);
            }
        }
    }
}
