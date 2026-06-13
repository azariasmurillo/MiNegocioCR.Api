using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Domain.Pricing;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class GetVariantsByCatalogItemUseCase : IGetVariantsByCatalogItemUseCase
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly IVariantRepository _variantRepository;
        private readonly IAppDbContext _context;

        public GetVariantsByCatalogItemUseCase(
            ICatalogRepository catalogRepository,
            IVariantRepository variantRepository,
            IAppDbContext context)
        {
            _catalogRepository = catalogRepository;
            _variantRepository = variantRepository;
            _context = context;
        }

        public async Task<List<CatalogVariantListItemDto>> ExecuteAsync(Guid catalogItemId, Guid businessId)
        {
            if (catalogItemId == Guid.Empty)
                throw new ArgumentException("CatalogItemId is required.", nameof(catalogItemId));

            if (businessId == Guid.Empty)
                throw new ArgumentException("BusinessId is required.", nameof(businessId));

            var item = await _catalogRepository.GetItemByIdAsync(catalogItemId);
            if (item == null || item.BusinessId != businessId)
                throw new NotFoundException("CatalogItem", "Catalog item not found.");

            var businessDefault = await _context.Businesses
                .AsNoTracking()
                .Where(b => b.Id == item.BusinessId)
                .Select(b => b.DefaultProfitMargin)
                .FirstOrDefaultAsync();

            var variants = await _variantRepository.GetVariantsWithOptionDetailsByCatalogItemIdAsync(catalogItemId);
            var variantIds = variants.Select(v => v.Id).ToList();
            var initialByVariant = await _variantRepository.GetInitialStockQuantitiesAsync(variantIds);
            var primaryImageByVariant = await GetPrimaryImageUrlsAsync(businessId, variantIds);

            var result = new List<CatalogVariantListItemDto>(variants.Count);
            foreach (var v in variants)
            {
                initialByVariant.TryGetValue(v.Id, out var initialStock);

                var links = v.VariantOptionValues
                    .OrderBy(l => l.CatalogOptionValue.CatalogOption.Name)
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
                    CatalogItemName = item.Name,
                    Sku = v.SKU,
                    Price = CrcSalePriceNormalizer.NormalizeSalePriceColones(v.Price),
                    CostPrice = v.CostPrice,
                    ProfitMargin = v.ProfitMargin,
                    EffectiveProfitMargin = v.ResolveProfitMargin(businessDefault),
                    InitialStock = initialStock,
                    CurrentStock = v.StockQuantity,
                    OptionValueIds = optionValueIds,
                    OptionValueLabels = optionValueLabels,
                    CreatedAt = v.CreatedAt,
                    IsActive = v.IsActive,
                    PrimaryImageUrl = primaryImageByVariant.GetValueOrDefault(v.Id)
                });
            }

            return result;
        }

        private async Task<Dictionary<Guid, string>> GetPrimaryImageUrlsAsync(Guid businessId, List<Guid> variantIds)
        {
            if (variantIds.Count == 0)
                return new Dictionary<Guid, string>();

            var rows = await _context.CatalogVariantImages
                .AsNoTracking()
                .Where(i => i.BusinessId == businessId && variantIds.Contains(i.CatalogVariantId))
                .OrderByDescending(i => i.IsPrimary)
                .ThenBy(i => i.SortOrder)
                .ThenBy(i => i.CreatedAt)
                .Select(i => new { i.CatalogVariantId, Url = i.ThumbnailImageUrl ?? i.ImageUrl })
                .ToListAsync();

            return rows
                .GroupBy(r => r.CatalogVariantId)
                .ToDictionary(g => g.Key, g => g.First().Url);
        }
    }
}
