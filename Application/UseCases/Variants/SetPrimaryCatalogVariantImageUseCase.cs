using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Variants;

public class SetPrimaryCatalogVariantImageUseCase : ISetPrimaryCatalogVariantImageUseCase
{
    private readonly IAppDbContext _context;

    public SetPrimaryCatalogVariantImageUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CatalogVariantImageDto>> ExecuteAsync(
        Guid businessId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (imageId == Guid.Empty)
            throw new ArgumentException("ImageId is required.", nameof(imageId));

        var image = await _context.CatalogVariantImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.BusinessId == businessId, cancellationToken);

        if (image == null)
            throw new NotFoundException("CatalogVariantImage", "Image not found.");

        var siblings = await _context.CatalogVariantImages
            .Where(i => i.BusinessId == businessId && i.CatalogVariantId == image.CatalogVariantId)
            .ToListAsync(cancellationToken);

        foreach (var s in siblings)
            s.IsPrimary = s.Id == image.Id;

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.CatalogVariantImages
            .AsNoTracking()
            .Where(i => i.BusinessId == businessId && i.CatalogVariantId == image.CatalogVariantId)
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.CreatedAt)
            .Select(i => new CatalogVariantImageDto { Id = i.Id, ImageUrl = i.ImageUrl, IsPrimary = i.IsPrimary })
            .ToListAsync(cancellationToken);
    }
}
