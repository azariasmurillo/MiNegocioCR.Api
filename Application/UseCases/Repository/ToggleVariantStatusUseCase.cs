using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class ToggleVariantStatusUseCase : IToggleVariantStatusUseCase
    {
        private readonly IVariantRepository _variantRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public ToggleVariantStatusUseCase(
            IVariantRepository variantRepository,
            IInventoryRepository inventoryRepository)
        {
            _variantRepository = variantRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task ExecuteAsync(Guid variantId, ToggleVariantStatusRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (variantId == Guid.Empty)
                throw new ArgumentException("Variant id is required.", nameof(variantId));

            if (request.BusinessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(request));

            var variant = await _variantRepository.GetVariantAsync(variantId, request.BusinessId);
            if (variant == null)
                throw new NotFoundException("CatalogVariant", "Variant not found.");

            if (!request.IsActive && variant.StockQuantity > 0)
            {
                var stockToClear = variant.StockQuantity;
                await _inventoryRepository.AddMovementAsync(new InventoryMovement
                {
                    Id = Guid.NewGuid(),
                    BusinessId = request.BusinessId,
                    CatalogVariantId = variantId,
                    Quantity = -stockToClear,
                    Type = InventoryMovementType.Adjustment,
                    Notes = "Presentación desactivada",
                    CreatedAt = DateTime.UtcNow
                });
                variant.StockQuantity = 0;
            }

            variant.IsActive = request.IsActive;
            await _variantRepository.UpdateAsync(variant);
        }
    }
}
