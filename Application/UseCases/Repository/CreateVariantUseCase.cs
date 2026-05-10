using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class CreateVariantUseCase : ICreateVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICatalogOptionValueRepository _optionValueRepository;
        private readonly ICatalogVariantOptionValueRepository _variantOptionValueRepository;

        public CreateVariantUseCase(
            IVariantRepository variantRepository,
            IInventoryRepository inventoryRepository,
            ICatalogRepository catalogRepository,
            ICatalogOptionValueRepository optionValueRepository,
            ICatalogVariantOptionValueRepository variantOptionValueRepository)
        {
            _variantRepository = variantRepository;
            _inventoryRepository = inventoryRepository;
            _catalogRepository = catalogRepository;
            _optionValueRepository = optionValueRepository;
            _variantOptionValueRepository = variantOptionValueRepository;
        }

        public async Task<Guid> ExecuteAsync(CreateVariantRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.CatalogItemId == Guid.Empty)
                throw new ArgumentException("CatalogItemId is required.", nameof(request));

            var catalogItem = await _catalogRepository.GetItemByIdAsync(request.CatalogItemId);
            if (catalogItem == null)
                throw new NotFoundException("CatalogItem", "Catalog item not found");

            var optionValueIds = request.OptionValueIds ?? new List<Guid>();
            if (optionValueIds.Count != optionValueIds.Distinct().Count())
                throw new ArgumentException("Duplicate option values are not allowed.", nameof(request));

            var sortedValueIds = optionValueIds.Distinct().OrderBy(x => x).ToList();

            if (request.ProfitMargin.HasValue && request.ProfitMargin.Value < 0)
                throw new ArgumentException("ProfitMargin must be greater than or equal to zero.", nameof(request));

            if (request.CostPrice < 0)
                throw new ArgumentException("CostPrice cannot be negative.", nameof(request));

            if (sortedValueIds.Count > 0)
            {
                var optionValues = await _optionValueRepository.GetByIdsWithCatalogOptionAsync(sortedValueIds);
                if (optionValues.Count != sortedValueIds.Count)
                    throw new NotFoundException("CatalogOptionValue", "One or more option values were not found.");

                foreach (var ov in optionValues)
                {
                    if (ov.CatalogOption.CatalogItemId != request.CatalogItemId)
                    {
                        throw new ArgumentException(
                            "All option values must belong to the same catalog item.",
                            nameof(request));
                    }
                }
            }

            if (await _variantOptionValueRepository.ExistsVariantWithSameOptionValueCombinationAsync(
                    request.CatalogItemId,
                    sortedValueIds))
            {
                throw new ArgumentException(
                    sortedValueIds.Count > 0
                        ? "A variant with this option combination already exists."
                        : "This catalog item already has a variant without option values.",
                    nameof(request));
            }

            if (!string.IsNullOrWhiteSpace(request.SKU) &&
                await _variantRepository.ExistsSkuForCatalogItemAsync(request.CatalogItemId, request.SKU))
            {
                throw new ArgumentException(
                    "A variant with this SKU already exists for this catalog item.",
                    nameof(request.SKU));
            }

            var resolvedPrice = CatalogVariantPriceResolver.ResolvePersistedPrice(
                request.SetPriceManually,
                request.CostPrice,
                request.ProfitMargin,
                request.Price);
            if (resolvedPrice < 0)
                throw new ArgumentException("Resolved price cannot be negative.", nameof(request));

            var variant = new CatalogVariant
            {
                Id = Guid.NewGuid(),
                CatalogItemId = request.CatalogItemId,
                SKU = string.IsNullOrWhiteSpace(request.SKU) ? null : request.SKU.Trim(),
                Price = resolvedPrice,
                CostPrice = request.CostPrice,
                ProfitMargin = request.ProfitMargin,
                StockQuantity = request.InitialStock,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _variantRepository.AddVariantAsync(variant);

            if (sortedValueIds.Count > 0)
            {
                var links = sortedValueIds.Select(valueId => new CatalogVariantOptionValue
                {
                    Id = Guid.NewGuid(),
                    CatalogVariantId = variant.Id,
                    CatalogOptionValueId = valueId
                }).ToList();

                await _variantOptionValueRepository.AddRangeAsync(links);
            }

            if (request.InitialStock > 0)
            {
                var movement = new InventoryMovement
                {
                    Id = Guid.NewGuid(),
                    BusinessId = catalogItem.BusinessId,
                    CatalogVariantId = variant.Id,
                    Quantity = request.InitialStock,
                    Type = InventoryMovementType.Purchase,
                    Notes = InventoryMovementNotes.InitialStock,
                    CreatedAt = DateTime.UtcNow
                };

                await _inventoryRepository.AddMovementAsync(movement);
            }

            return variant.Id;
        }
    }
}
