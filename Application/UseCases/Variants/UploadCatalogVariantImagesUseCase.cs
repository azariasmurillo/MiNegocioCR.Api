using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Variants;

public class UploadCatalogVariantImagesUseCase : IUploadCatalogVariantImagesUseCase
{
    public const int MaxImagesPerVariant = 3;

    private readonly IAppDbContext _context;
    private readonly IVariantImageStorageService _storage;

    public UploadCatalogVariantImagesUseCase(IAppDbContext context, IVariantImageStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<IReadOnlyList<CatalogVariantImageDto>> ExecuteAsync(
        Guid businessId,
        Guid catalogVariantId,
        IReadOnlyList<CatalogVariantImageUploadInput> files,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (catalogVariantId == Guid.Empty)
            throw new ArgumentException("CatalogVariantId is required.", nameof(catalogVariantId));
        if (files == null || files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(files));

        var variantExists = await _context.CatalogVariants
            .AnyAsync(v => v.Id == catalogVariantId && v.CatalogItem.BusinessId == businessId, cancellationToken);
        if (!variantExists)
            throw new NotFoundException("CatalogVariant", "Catalog variant not found.");

        var existingImages = await _context.CatalogVariantImages
            .Where(i => i.CatalogVariantId == catalogVariantId && i.BusinessId == businessId)
            .ToListAsync(cancellationToken);

        if (existingImages.Count + files.Count > MaxImagesPerVariant)
            throw new ArgumentException($"A catalog variant can have at most {MaxImagesPerVariant} images.");

        var occupiedSlots = ResolveOccupiedSlots(existingImages);

        var index = 0;
        foreach (var file in files)
        {
            var sortOrder = TakeNextSortOrder(occupiedSlots);
            var isPrimary = sortOrder == 1 || (existingImages.Count == 0 && index == 0);
            var imageUrl = await _storage.UploadAsync(catalogVariantId, file.Stream, file.ContentType, cancellationToken);

            var entity = new CatalogVariantImage
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                CatalogVariantId = catalogVariantId,
                ImageUrl = imageUrl,
                SortOrder = sortOrder,
                IsPrimary = isPrimary,
                CreatedAt = DateTime.UtcNow
            };

            _context.CatalogVariantImages.Add(entity);
            index++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await ProjectOrderedListAsync(businessId, catalogVariantId, cancellationToken);
    }

    internal static HashSet<int> ResolveOccupiedSlots(IReadOnlyList<CatalogVariantImage> existingImages)
    {
        var occupied = existingImages
            .Where(i => i.SortOrder is >= 1 and <= MaxImagesPerVariant)
            .Select(i => i.SortOrder)
            .ToHashSet();

        var legacyCount = existingImages.Count(i => i.SortOrder is < 1 or > MaxImagesPerVariant);
        for (var slot = 1; legacyCount > 0 && slot <= MaxImagesPerVariant; slot++)
        {
            if (occupied.Add(slot))
            {
                legacyCount--;
            }
        }

        return occupied;
    }

    internal static int TakeNextSortOrder(HashSet<int> occupiedSlots)
    {
        for (var slot = 1; slot <= MaxImagesPerVariant; slot++)
        {
            if (occupiedSlots.Add(slot))
            {
                return slot;
            }
        }

        throw new InvalidOperationException($"No free image slot (max {MaxImagesPerVariant}).");
    }

    private async Task<IReadOnlyList<CatalogVariantImageDto>> ProjectOrderedListAsync(
        Guid businessId,
        Guid catalogVariantId,
        CancellationToken cancellationToken)
    {
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
