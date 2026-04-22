using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class GetVariantsByBusinessUseCase : IGetVariantsByBusinessUseCase
    {
        private readonly IVariantRepository _variantRepository;

        public GetVariantsByBusinessUseCase(IVariantRepository variantRepository)
        {
            _variantRepository = variantRepository;
        }

        public async Task<List<CatalogVariantListItemDto>> ExecuteAsync(
            Guid businessId,
            Guid? catalogItemId = null,
            string? search = null)
        {
            if (businessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(businessId));

            if (catalogItemId.HasValue && catalogItemId.Value == Guid.Empty)
                throw new ArgumentException("CatalogItemId must be valid when provided.", nameof(catalogItemId));

            var variants = await _variantRepository.GetVariantsWithOptionDetailsByBusinessAsync(
                businessId,
                catalogItemId,
                search);

            var variantIds = variants.Select(v => v.Id).ToList();
            var initialByVariant = await _variantRepository.GetInitialStockQuantitiesAsync(variantIds);

            var result = new List<CatalogVariantListItemDto>(variants.Count);
            foreach (var v in variants)
            {
                initialByVariant.TryGetValue(v.Id, out var initialStock);

                var links = v.VariantOptionValues
                    .OrderBy(l => l.CatalogOptionValue.CatalogOption.Name)
                    .ThenBy(l => l.CatalogOptionValue.Value)
                    .ToList();

                var optionValueIds = new List<Guid>(links.Count);
                var optionValueLabels = new List<string>(links.Count);
                foreach (var link in links)
                {
                    var ov = link.CatalogOptionValue;
                    optionValueIds.Add(ov.Id);
                    optionValueLabels.Add($"{ov.CatalogOption.Name}: {ov.Value}");
                }

                result.Add(new CatalogVariantListItemDto
                {
                    Id = v.Id,
                    CatalogItemId = v.CatalogItemId,
                    CatalogItemName = v.CatalogItem.Name,
                    Sku = v.SKU,
                    Price = v.Price,
                    InitialStock = initialStock,
                    CurrentStock = v.StockQuantity,
                    OptionValueIds = optionValueIds,
                    OptionValueLabels = optionValueLabels,
                    CreatedAt = v.CreatedAt
                });
            }

            return result;
        }
    }
}
