using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class CreateVariantUseCase : ICreateVariantUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public CreateVariantUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task<Guid> ExecuteAsync(
            Guid catalogItemId,
            string sku,
            decimal price,
            int initialStock)
        {
            var variant = new CatalogVariant
            {
                Id = Guid.NewGuid(),
                CatalogItemId = catalogItemId,
                SKU = sku,
                Price = price,
                StockQuantity = initialStock,
                IsActive = true
            };

            await _variantRepository.AddVariantAsync(variant);

            return variant.Id;
        }
    }
}
