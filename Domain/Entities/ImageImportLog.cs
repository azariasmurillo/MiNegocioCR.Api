using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities;

public class ImageImportLog
{
    public Guid Id { get; set; }

    public Guid BatchId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? ParsedSku { get; set; }

    public int? SortOrder { get; set; }

    public Guid? CatalogVariantId { get; set; }

    public ImageImportLogStatus Status { get; set; }

    public string? Message { get; set; }

    public int? DurationMs { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ImageImportBatch Batch { get; set; } = null!;
}
