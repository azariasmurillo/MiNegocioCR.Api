using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class DeleteVariantUseCase : IDeleteVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public DeleteVariantUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task ExecuteAsync(Guid variantId, Guid businessId)
        {
            if (variantId == Guid.Empty)
                throw new ArgumentException("Variant id is required.", nameof(variantId));

            if (businessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(businessId));

            var variant = await _variantRepository.GetVariantAsync(variantId, businessId);
            if (variant == null)
                throw new NotFoundException("CatalogVariant", "Variant not found.");

            if (await _variantRepository.ExistsInInventoryAsync(variantId))
                throw new InvalidOperationException("Variant has inventory history and cannot be deleted");

            if (await _variantRepository.ExistsInSalesAsync(variantId))
                throw new InvalidOperationException("Variant has sales and cannot be deleted");

            if (await _variantRepository.ExistsInCreditsAsync(variantId))
                throw new InvalidOperationException("Variant has credit charges and cannot be deleted");

            if (await _variantRepository.ExistsInRepairOrdersAsync(variantId))
                throw new InvalidOperationException("Variant has repair orders and cannot be deleted");

            if (await _variantRepository.ExistsInPurchasesAsync(variantId))
                throw new InvalidOperationException("Variant has purchases and cannot be deleted");

            await _variantRepository.DeleteAsync(variant);
        }
    }
}
