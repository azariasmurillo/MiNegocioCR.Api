using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class UpdateVariantUseCase : IUpdateVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public UpdateVariantUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task ExecuteAsync(Guid variantId, UpdateVariantRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (variantId == Guid.Empty)
                throw new ArgumentException("Variant id is required.", nameof(variantId));

            if (request.BusinessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(request));

            if (request.CostPrice < 0)
                throw new ArgumentException("CostPrice cannot be negative.", nameof(request));

            var variant = await _variantRepository.GetVariantAsync(variantId, request.BusinessId);
            if (variant == null)
                throw new NotFoundException("CatalogVariant", "Variant not found.");

            if (!string.IsNullOrWhiteSpace(request.SKU) &&
                await _variantRepository.ExistsSkuForCatalogItemAsync(
                    variant.CatalogItemId,
                    request.SKU,
                    variant.Id))
            {
                throw new ArgumentException(
                    "A variant with this SKU already exists for this catalog item.",
                    nameof(request.SKU));
            }

            variant.SKU = string.IsNullOrWhiteSpace(request.SKU) ? null : request.SKU.Trim();
            variant.CostPrice = request.CostPrice;

            if (request.SetProfitMargin)
            {
                if (request.ProfitMargin.HasValue && request.ProfitMargin.Value < 0)
                    throw new ArgumentException("ProfitMargin must be greater than or equal to zero.", nameof(request));
                variant.ProfitMargin = request.ProfitMargin;
            }

            variant.Price = CatalogVariantPriceResolver.ResolvePersistedPrice(
                request.SetPriceManually,
                variant.CostPrice,
                variant.ProfitMargin,
                request.Price);

            if (variant.Price < 0)
                throw new ArgumentException("Resolved price cannot be negative.", nameof(request));

            await _variantRepository.UpdateAsync(variant);
        }
    }
}
