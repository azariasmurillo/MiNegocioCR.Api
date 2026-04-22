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

            if (request.Price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(request));

            var variant = await _variantRepository.GetVariantAsync(variantId, request.BusinessId);
            if (variant == null)
                throw new NotFoundException("CatalogVariant", "Variant not found.");

            variant.SKU = request.SKU;
            variant.Price = request.Price;

            await _variantRepository.UpdateAsync(variant);
        }
    }
}
