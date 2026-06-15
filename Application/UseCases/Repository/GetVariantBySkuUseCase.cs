using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Domain.Pricing;

namespace MiNegocioCR.Api.Application.UseCases.Repository;

public class GetVariantBySkuUseCase : IGetVariantBySkuUseCase
{
    private readonly IVariantRepository _variantRepository;
    private readonly IAppDbContext _context;

    public GetVariantBySkuUseCase(IVariantRepository variantRepository, IAppDbContext context)
    {
        _variantRepository = variantRepository;
        _context = context;
    }

    public async Task<VariantBySkuLookupDto> ExecuteAsync(
        Guid businessId,
        string sku,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));

        var variant = await _variantRepository.GetVariantWithOptionDetailsByBusinessAndSkuAsync(
            businessId,
            sku,
            cancellationToken);

        if (variant == null)
            throw new NotFoundException("CatalogVariant", $"No hay variante con el SKU «{sku.Trim()}» en tu negocio.");

        var businessDefault = await _context.Businesses
            .AsNoTracking()
            .Where(b => b.Id == businessId)
            .Select(b => b.DefaultProfitMargin)
            .FirstOrDefaultAsync(cancellationToken);

        var links = variant.VariantOptionValues
            .OrderBy(l => l.CatalogOptionValue.CatalogOption.Name)
            .ThenBy(l => l.CatalogOptionValue.Value)
            .ToList();

        var optionValueLabels = links
            .Select(l => $"{l.CatalogOptionValue.CatalogOption.Name}: {l.CatalogOptionValue.Value}")
            .ToList();

        var presentationLabel = optionValueLabels.Count > 0
            ? string.Join(" · ", links.Select(l => l.CatalogOptionValue.Value))
            : null;

        var primaryImageUrl = await _context.CatalogVariantImages
            .AsNoTracking()
            .Where(i => i.BusinessId == businessId && i.CatalogVariantId == variant.Id)
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .Select(i => i.ThumbnailImageUrl ?? i.ImageUrl)
            .FirstOrDefaultAsync(cancellationToken);

        var imageCount = await _context.CatalogVariantImages
            .AsNoTracking()
            .CountAsync(i => i.BusinessId == businessId && i.CatalogVariantId == variant.Id, cancellationToken);

        return new VariantBySkuLookupDto
        {
            VariantId = variant.Id,
            CatalogItemId = variant.CatalogItemId,
            CatalogItemName = variant.CatalogItem.Name,
            PresentationLabel = presentationLabel,
            Sku = variant.SKU,
            CurrentStock = variant.StockQuantity,
            Price = CrcSalePriceNormalizer.NormalizeSalePriceColones(variant.Price),
            CostPrice = variant.CostPrice,
            ProfitMargin = variant.ProfitMargin,
            EffectiveProfitMargin = variant.ResolveProfitMargin(businessDefault),
            PrimaryImageUrl = primaryImageUrl,
            ImageCount = imageCount,
            IsActive = variant.IsActive,
            OptionValueLabels = optionValueLabels,
        };
    }
}
