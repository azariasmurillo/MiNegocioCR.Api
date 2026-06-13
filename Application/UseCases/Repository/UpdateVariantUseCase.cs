using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class UpdateVariantUseCase : IUpdateVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;
        private readonly IBusinessRepository _businessRepository;

        public UpdateVariantUseCase(IVariantRepository variantRepository, IBusinessRepository businessRepository)
        {
            _variantRepository = variantRepository;
            _businessRepository = businessRepository;
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
                await _variantRepository.ExistsSkuForBusinessAsync(
                    request.BusinessId,
                    request.SKU,
                    variant.Id))
            {
                throw new ArgumentException(
                    $"Ya existe otra variante con el SKU «{request.SKU.Trim()}» en tu negocio.",
                    nameof(request.SKU));
            }

            SkuNormalizer.Apply(variant, request.BusinessId, request.SKU);
            variant.CostPrice = request.CostPrice;

            if (request.SetProfitMargin)
            {
                if (request.ProfitMargin.HasValue && request.ProfitMargin.Value < 0)
                    throw new ArgumentException("ProfitMargin must be greater than or equal to zero.", nameof(request));
                variant.ProfitMargin = request.ProfitMargin;
            }

            var business = await _businessRepository.GetByIdAsync(request.BusinessId);
            if (business == null)
                throw new NotFoundException("Business", "Business not found.");

            var taxRate = business.TaxRatePercent;
            if (taxRate < 0)
                throw new ArgumentException("Business tax rate cannot be negative.");

            variant.Price = CatalogVariantPriceResolver.ResolvePersistedPrice(
                request.SetPriceManually,
                variant.CostPrice,
                variant.ProfitMargin,
                taxRate,
                request.Price);

            if (variant.Price < 0)
                throw new ArgumentException("Resolved price cannot be negative.", nameof(request));

            await _variantRepository.UpdateAsync(variant);
        }
    }
}
