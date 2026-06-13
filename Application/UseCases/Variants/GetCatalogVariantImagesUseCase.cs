using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Variants;

public class GetCatalogVariantImagesUseCase : IGetCatalogVariantImagesUseCase
{
    private readonly IAppDbContext _context;

    public GetCatalogVariantImagesUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CatalogVariantImageDto>> ExecuteAsync(
        Guid businessId,
        Guid catalogVariantId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (catalogVariantId == Guid.Empty)
            throw new ArgumentException("CatalogVariantId is required.", nameof(catalogVariantId));

        var variantExists = await _context.CatalogVariants
            .AnyAsync(v => v.Id == catalogVariantId && v.CatalogItem.BusinessId == businessId, cancellationToken);
        if (!variantExists)
            throw new NotFoundException("CatalogVariant", "Catalog variant not found.");

        return await _context.CatalogVariantImages
            .AsNoTracking()
            .Where(i => i.BusinessId == businessId && i.CatalogVariantId == catalogVariantId)
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.SortOrder)
            .ThenBy(i => i.CreatedAt)
            .Select(i => new CatalogVariantImageDto { Id = i.Id, ImageUrl = i.ImageUrl, IsPrimary = i.IsPrimary })
            .ToListAsync(cancellationToken);
    }
}
