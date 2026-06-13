using System.Diagnostics;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Configuration;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.Variants;

public class ImageImportBatchProcessor : IImageImportBatchProcessor
{
    private static readonly TimeSpan StaleProcessingThreshold = TimeSpan.FromMinutes(30);

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
    };

    private readonly IAppDbContext _context;
    private readonly IVariantRepository _variantRepository;
    private readonly IProductImageEnhancerService _enhancer;
    private readonly IVariantImageStorageService _storage;
    private readonly VariantImageImportOptions _options;
    private readonly ILogger<ImageImportBatchProcessor> _logger;

    public ImageImportBatchProcessor(
        IAppDbContext context,
        IVariantRepository variantRepository,
        IProductImageEnhancerService enhancer,
        IVariantImageStorageService storage,
        IOptions<VariantImageImportOptions> options,
        ILogger<ImageImportBatchProcessor> logger)
    {
        _context = context;
        _variantRepository = variantRepository;
        _enhancer = enhancer;
        _storage = storage;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default)
    {
        await RecoverStaleProcessingBatchesAsync(cancellationToken);

        var batchId = await TryClaimNextBatchIdAsync(cancellationToken);
        if (batchId == null)
            return false;

        var batch = await _context.ImageImportBatches
            .FirstOrDefaultAsync(b => b.Id == batchId.Value, cancellationToken);

        if (batch == null || batch.Status != ImageImportBatchStatus.Processing)
            return false;

        try
        {
            await ProcessBatchAsync(batch, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image import batch {BatchId} failed.", batch.Id);
            batch.Status = ImageImportBatchStatus.Failed;
            batch.SummaryMessage = "Error inesperado al procesar el lote.";
            batch.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            TryDeleteStagingZip(batch.StagingZipPath);
        }

        return true;
    }

    private async Task ProcessBatchAsync(ImageImportBatch batch, CancellationToken cancellationToken)
    {
        if (!File.Exists(batch.StagingZipPath))
        {
            batch.Status = ImageImportBatchStatus.Failed;
            batch.SummaryMessage = "Archivo ZIP de staging no encontrado.";
            batch.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        using var archive = ZipFile.OpenRead(batch.StagingZipPath);
        var entries = archive.Entries
            .Where(e => !string.IsNullOrEmpty(e.Name))
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (entries.Count > _options.MaxFilesPerZip)
        {
            batch.Status = ImageImportBatchStatus.Failed;
            batch.SummaryMessage = $"El ZIP supera el máximo de {_options.MaxFilesPerZip} archivos.";
            batch.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        batch.TotalFiles = entries.Count;
        batch.ProcessedFiles = 0;
        batch.SuccessCount = 0;
        batch.SkippedCount = 0;
        batch.ErrorCount = 0;
        await _context.SaveChangesAsync(cancellationToken);

        var seenSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ProcessEntryAsync(batch, entry, seenSlots, cancellationToken);
            batch.ProcessedFiles++;
            await _context.SaveChangesAsync(cancellationToken);
        }

        batch.Status = batch.ErrorCount > 0
            ? ImageImportBatchStatus.CompletedWithErrors
            : ImageImportBatchStatus.Completed;
        batch.SummaryMessage = $"Procesados {batch.ProcessedFiles}: {batch.SuccessCount} ok, "
                               + $"{batch.SkippedCount} omitidos, {batch.ErrorCount} errores.";
        batch.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessEntryAsync(
        ImageImportBatch batch,
        ZipArchiveEntry entry,
        HashSet<string> seenSlots,
        CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var fileName = Path.GetFileName(entry.FullName);

        if (!SkuImageFileNameParser.TryParse(fileName, out var parsed))
        {
            await AddLogAsync(
                batch,
                fileName,
                null,
                null,
                null,
                ImageImportLogStatus.InvalidFileName,
                "Nombre inválido. Usá {SKU}_1.jpg … _3.",
                sw,
                cancellationToken);
            batch.ErrorCount++;
            return;
        }

        var ext = Path.GetExtension(fileName);
        if (!AllowedImageExtensions.Contains(ext))
        {
            await AddLogAsync(
                batch,
                fileName,
                parsed.Sku,
                parsed.SortOrder,
                null,
                ImageImportLogStatus.UnsupportedFormat,
                "Formato no soportado.",
                sw,
                cancellationToken);
            batch.ErrorCount++;
            return;
        }

        if (entry.Length > _options.MaxImageBytes)
        {
            await AddLogAsync(
                batch,
                fileName,
                parsed.Sku,
                parsed.SortOrder,
                null,
                ImageImportLogStatus.ProcessingFailed,
                "La imagen supera el tamaño máximo permitido.",
                sw,
                cancellationToken);
            batch.ErrorCount++;
            return;
        }

        var slotKey = $"{SkuNormalizer.ToNormalizedKey(parsed.Sku)}:{parsed.SortOrder}";
        if (!seenSlots.Add(slotKey))
        {
            await AddLogAsync(
                batch,
                fileName,
                parsed.Sku,
                parsed.SortOrder,
                null,
                ImageImportLogStatus.DuplicateSlotInZip,
                "SKU y slot duplicados en el ZIP.",
                sw,
                cancellationToken);
            batch.ErrorCount++;
            return;
        }

        var variant = await _variantRepository.FindByBusinessAndSkuAsync(batch.BusinessId, parsed.Sku);
        if (variant == null)
        {
            await AddLogAsync(
                batch,
                fileName,
                parsed.Sku,
                parsed.SortOrder,
                null,
                ImageImportLogStatus.VariantNotFound,
                "No hay variante con ese SKU en tu negocio.",
                sw,
                cancellationToken);
            batch.ErrorCount++;
            return;
        }

        var existingAtSlot = await _context.CatalogVariantImages
            .FirstOrDefaultAsync(
                i => i.BusinessId == batch.BusinessId
                     && i.CatalogVariantId == variant.Id
                     && i.SortOrder == parsed.SortOrder,
                cancellationToken);

        if (existingAtSlot != null && !batch.ReplaceExisting)
        {
            await AddLogAsync(
                batch,
                fileName,
                parsed.Sku,
                parsed.SortOrder,
                variant.Id,
                ImageImportLogStatus.SkippedExisting,
                "Ya existe imagen en ese slot. Activá reemplazar para sobrescribir.",
                sw,
                cancellationToken);
            batch.SkippedCount++;
            return;
        }

        var imageCount = await _context.CatalogVariantImages
            .CountAsync(i => i.BusinessId == batch.BusinessId && i.CatalogVariantId == variant.Id, cancellationToken);

        if (existingAtSlot == null && imageCount >= _options.MaxImagesPerVariant)
        {
            await AddLogAsync(
                batch,
                fileName,
                parsed.Sku,
                parsed.SortOrder,
                variant.Id,
                ImageImportLogStatus.MaxImagesExceeded,
                $"La variante ya tiene {_options.MaxImagesPerVariant} imágenes.",
                sw,
                cancellationToken);
            batch.ErrorCount++;
            return;
        }

        try
        {
            await using var entryStream = entry.Open();
            using var buffer = new MemoryStream();
            await entryStream.CopyToAsync(buffer, cancellationToken);
            buffer.Position = 0;

            var enhanceOptions = new ProductImageEnhanceOptions
            {
                MarketplaceStyle = batch.MarketplaceStyle,
                UseBackgroundRemoval = batch.UseBackgroundRemoval,
                WebpQuality = _options.WebpQuality,
            };

            ProductImageEnhanceResult enhanced;
            try
            {
                enhanced = await _enhancer.EnhanceAsync(buffer, enhanceOptions, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                await AddLogAsync(
                    batch,
                    fileName,
                    parsed.Sku,
                    parsed.SortOrder,
                    variant.Id,
                    ImageImportLogStatus.ProcessingFailed,
                    ex.Message,
                    sw,
                    cancellationToken);
                batch.ErrorCount++;
                return;
            }

            if (existingAtSlot != null)
            {
                await DeleteImageFilesAsync(existingAtSlot, cancellationToken);
                _context.CatalogVariantImages.Remove(existingAtSlot);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var imageId = Guid.NewGuid();
            ProcessedVariantImageUrls urls;
            try
            {
                await using var main = enhanced.Main;
                await using var mobile = enhanced.Mobile;
                await using var thumb = enhanced.Thumbnail;
                urls = await _storage.UploadProcessedAsync(
                    variant.Id,
                    imageId,
                    new ProcessedVariantImageStreams
                    {
                        Main = main,
                        Mobile = mobile,
                        Thumbnail = thumb,
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Storage failed for {FileName} batch {BatchId}.", fileName, batch.Id);
                await AddLogAsync(
                    batch,
                    fileName,
                    parsed.Sku,
                    parsed.SortOrder,
                    variant.Id,
                    ImageImportLogStatus.StorageFailed,
                    "No se pudo subir la imagen procesada.",
                    sw,
                    cancellationToken);
                batch.ErrorCount++;
                return;
            }
            finally
            {
                await enhanced.Main.DisposeAsync();
                await enhanced.Mobile.DisposeAsync();
                await enhanced.Thumbnail.DisposeAsync();
            }

            if (parsed.SortOrder == 1)
            {
                var primaries = await _context.CatalogVariantImages
                    .Where(i => i.BusinessId == batch.BusinessId && i.CatalogVariantId == variant.Id && i.IsPrimary)
                    .ToListAsync(cancellationToken);
                foreach (var p in primaries)
                    p.IsPrimary = false;
            }

            var entity = new CatalogVariantImage
            {
                Id = imageId,
                BusinessId = batch.BusinessId,
                CatalogVariantId = variant.Id,
                ImageUrl = urls.MainUrl,
                MobileImageUrl = urls.MobileUrl,
                ThumbnailImageUrl = urls.ThumbnailUrl,
                SortOrder = parsed.SortOrder,
                ImportBatchId = batch.Id,
                SourceFileName = fileName,
                IsPrimary = parsed.SortOrder == 1,
                CreatedAt = DateTime.UtcNow,
            };

            _context.CatalogVariantImages.Add(entity);
            await AddLogAsync(
                batch,
                fileName,
                parsed.Sku,
                parsed.SortOrder,
                variant.Id,
                ImageImportLogStatus.Success,
                null,
                sw,
                cancellationToken);
            batch.SuccessCount++;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Processing failed for {FileName} batch {BatchId}.", fileName, batch.Id);
            await AddLogAsync(
                batch,
                fileName,
                parsed.Sku,
                parsed.SortOrder,
                variant.Id,
                ImageImportLogStatus.ProcessingFailed,
                "Error al procesar la imagen.",
                sw,
                cancellationToken);
            batch.ErrorCount++;
        }
    }

    private async Task DeleteImageFilesAsync(CatalogVariantImage image, CancellationToken cancellationToken)
    {
        await _storage.DeleteByPublicUrlAsync(image.ImageUrl, cancellationToken);
        if (!string.IsNullOrWhiteSpace(image.MobileImageUrl))
            await _storage.DeleteByPublicUrlAsync(image.MobileImageUrl!, cancellationToken);
        if (!string.IsNullOrWhiteSpace(image.ThumbnailImageUrl))
            await _storage.DeleteByPublicUrlAsync(image.ThumbnailImageUrl!, cancellationToken);
    }

    private async Task AddLogAsync(
        ImageImportBatch batch,
        string fileName,
        string? parsedSku,
        int? sortOrder,
        Guid? catalogVariantId,
        ImageImportLogStatus status,
        string? message,
        Stopwatch sw,
        CancellationToken cancellationToken)
    {
        sw.Stop();
        _context.ImageImportLogs.Add(new ImageImportLog
        {
            Id = Guid.NewGuid(),
            BatchId = batch.Id,
            FileName = fileName,
            ParsedSku = parsedSku,
            SortOrder = sortOrder,
            CatalogVariantId = catalogVariantId,
            Status = status,
            Message = message,
            DurationMs = (int)sw.ElapsedMilliseconds,
            CreatedAt = DateTime.UtcNow,
        });
        await Task.CompletedTask;
    }

    private async Task<Guid?> TryClaimNextBatchIdAsync(CancellationToken cancellationToken)
    {
        var candidateId = await _context.ImageImportBatches
            .AsNoTracking()
            .Where(b => b.Status == ImageImportBatchStatus.Pending)
            .OrderBy(b => b.CreatedAt)
            .Select(b => b.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (candidateId == Guid.Empty)
            return null;

        if (_context.Database.IsRelational())
        {
            var claimed = await _context.ImageImportBatches
                .Where(b => b.Id == candidateId && b.Status == ImageImportBatchStatus.Pending)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(b => b.Status, ImageImportBatchStatus.Processing),
                    cancellationToken);

            return claimed == 1 ? candidateId : null;
        }

        var tracked = await _context.ImageImportBatches
            .FirstOrDefaultAsync(
                b => b.Id == candidateId && b.Status == ImageImportBatchStatus.Pending,
                cancellationToken);

        if (tracked == null)
            return null;

        tracked.Status = ImageImportBatchStatus.Processing;
        await _context.SaveChangesAsync(cancellationToken);
        return tracked.Id;
    }

    private async Task RecoverStaleProcessingBatchesAsync(CancellationToken cancellationToken)
    {
        var staleBefore = DateTime.UtcNow - StaleProcessingThreshold;
        var stale = await _context.ImageImportBatches
            .Where(b => b.Status == ImageImportBatchStatus.Processing && b.CreatedAt < staleBefore)
            .ToListAsync(cancellationToken);

        if (stale.Count == 0)
            return;

        foreach (var batch in stale)
        {
            batch.Status = ImageImportBatchStatus.Pending;
            batch.SummaryMessage = "Reintento automático tras procesamiento interrumpido.";
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogWarning("Recovered {Count} stale image import batches.", stale.Count);
    }

    private static void TryDeleteStagingZip(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best effort cleanup.
        }
    }
}
