using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Pricing;

namespace MiNegocioCR.Api.Application.UseCases.Repository
{
    public class GetVariantsByBusinessUseCase : IGetVariantsByBusinessUseCase
    {
        private readonly IVariantRepository _variantRepository;
        private readonly IAppDbContext _context;

        public GetVariantsByBusinessUseCase(IVariantRepository variantRepository, IAppDbContext context)
        {
            _variantRepository = variantRepository;
            _context = context;
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

            var businessDefault = await _context.Businesses
                .AsNoTracking()
                .Where(b => b.Id == businessId)
                .Select(b => b.DefaultProfitMargin)
                .FirstOrDefaultAsync();

            var variants = await _variantRepository.GetVariantsWithOptionDetailsByBusinessAsync(
                businessId,
                catalogItemId,
                search);

            var variantIds = variants.Select(v => v.Id).ToList();
            var initialByVariant = await _variantRepository.GetInitialStockQuantitiesAsync(variantIds);
            var primaryImageByVariant = await GetPrimaryImageUrlsAsync(businessId, variantIds);

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
