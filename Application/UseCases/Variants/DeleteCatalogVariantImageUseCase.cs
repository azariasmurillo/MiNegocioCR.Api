using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Variants;

public class DeleteCatalogVariantImageUseCase : IDeleteCatalogVariantImageUseCase
{
    private readonly IAppDbContext _context;
    private readonly IVariantImageStorageService _storage;

    public DeleteCatalogVariantImageUseCase(IAppDbContext context, IVariantImageStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task ExecuteAsync(Guid businessId, Guid imageId, CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (imageId == Guid.Empty)
            throw new ArgumentException("ImageId is required.", nameof(imageId));

        var image = await _context.CatalogVariantImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.BusinessId == businessId, cancellationToken);

        if (image == null)
            throw new NotFoundException("CatalogVariantImage", "Image not found.");

        var variantId = image.CatalogVariantId;
        var wasPrimary = image.IsPrimary;

        await _storage.DeleteByPublicUrlAsync(image.ImageUrl, cancellationToken);
        _context.CatalogVariantImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);

        if (!wasPrimary)
            return;

        var remaining = await _context.CatalogVariantImages
            .Where(i => i.BusinessId == businessId && i.CatalogVariantId == variantId)
            .OrderBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        if (remaining.Count == 0)
            return;

        remaining[0].IsPrimary = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
