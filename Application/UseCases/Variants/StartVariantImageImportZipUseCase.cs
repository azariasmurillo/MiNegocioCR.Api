using System.IO.Compression;
using Microsoft.Extensions.Options;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Configuration;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Variants;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.UseCases.Variants;

public class StartVariantImageImportZipUseCase : IStartVariantImageImportZipUseCase
{
    private readonly IAppDbContext _context;
    private readonly VariantImageImportOptions _options;

    public StartVariantImageImportZipUseCase(
        IAppDbContext context,
        IOptions<VariantImageImportOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<Guid> ExecuteAsync(
        StartVariantImageImportZipInput input,
        CancellationToken cancellationToken = default)
    {
        if (input.BusinessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(input));
        if (input.CreatedByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId is required.", nameof(input));
        if (input.ZipStream == null)
            throw new ArgumentException("ZipStream is required.", nameof(input));
        if (input.ZipLength <= 0)
            throw new ArgumentException("ZIP vacío.", nameof(input));
        if (input.ZipLength > _options.MaxZipBytes)
            throw new ArgumentException($"El ZIP no puede superar {_options.MaxZipBytes / (1024 * 1024)} MB.");

        var style = string.IsNullOrWhiteSpace(input.MarketplaceStyle)
            ? MarketplaceStylePresets.WhiteV1
            : input.MarketplaceStyle.Trim();

        if (!string.Equals(style, MarketplaceStylePresets.WhiteV1, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(style, MarketplaceStylePresets.SoftV1, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Estilo no soportado: {style}.");
        }

        ValidateZipStructure(input.ZipStream);

        var batchId = Guid.NewGuid();
        var stagingDir = ResolveStagingDirectory();
        Directory.CreateDirectory(stagingDir);
        var stagingPath = Path.Combine(stagingDir, $"{batchId:N}.zip");

        await using (var fileStream = new FileStream(
                         stagingPath,
                         FileMode.Create,
                         FileAccess.Write,
                         FileShare.None,
                         bufferSize: 81920,
                         useAsync: true))
        {
            if (input.ZipStream.CanSeek)
                input.ZipStream.Position = 0;
            await input.ZipStream.CopyToAsync(fileStream, cancellationToken);
        }

        var totalFiles = CountImageEntries(stagingPath);

        var batch = new ImageImportBatch
        {
            Id = batchId,
            BusinessId = input.BusinessId,
            CreatedByUserId = input.CreatedByUserId,
            OriginalFileName = Path.GetFileName(input.OriginalFileName),
            StagingZipPath = stagingPath,
            ReplaceExisting = input.ReplaceExisting,
            UseBackgroundRemoval = input.UseBackgroundRemoval,
            MarketplaceStyle = style,
            Status = ImageImportBatchStatus.Pending,
            TotalFiles = totalFiles,
            CreatedAt = DateTime.UtcNow,
        };

        _context.ImageImportBatches.Add(batch);
        await _context.SaveChangesAsync(cancellationToken);
        return batchId;
    }

    private string ResolveStagingDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_options.StagingDirectory))
            return _options.StagingDirectory;

        return Path.Combine(Path.GetTempPath(), "MiNegocioCR", "image-import");
    }

    private static void ValidateZipStructure(Stream zipStream)
    {
        if (!zipStream.CanSeek)
            throw new ArgumentException("No se pudo validar el ZIP.");

        var start = zipStream.Position;
        try
        {
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true);
            if (archive.Entries.Count == 0)
                throw new ArgumentException("El ZIP no contiene archivos.");

            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var fullName = entry.FullName.Replace('\\', '/');
                if (fullName.Contains("../", StringComparison.Ordinal)
                    || fullName.StartsWith("../", StringComparison.Ordinal)
                    || fullName.Contains("/../", StringComparison.Ordinal))
                {
                    throw new ArgumentException("El ZIP contiene rutas no permitidas.");
                }
            }
        }
        catch (InvalidDataException)
        {
            throw new ArgumentException("El archivo no es un ZIP válido.");
        }
        finally
        {
            zipStream.Position = start;
        }
    }

    private static int CountImageEntries(string stagingPath)
    {
        using var archive = ZipFile.OpenRead(stagingPath);
        return archive.Entries.Count(e => !string.IsNullOrEmpty(e.Name));
    }
}
