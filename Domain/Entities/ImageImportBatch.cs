using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities;

public class ImageImportBatch
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StagingZipPath { get; set; } = string.Empty;

    public bool ReplaceExisting { get; set; }

    public bool UseBackgroundRemoval { get; set; }

    public string MarketplaceStyle { get; set; } = "marketplace-white-v1";

    public ImageImportBatchStatus Status { get; set; }

    public int TotalFiles { get; set; }

    public int ProcessedFiles { get; set; }

    public int SuccessCount { get; set; }

    public int SkippedCount { get; set; }

    public int ErrorCount { get; set; }

    public string? SummaryMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public Business Business { get; set; } = null!;

    public ICollection<ImageImportLog> Logs { get; set; } = new List<ImageImportLog>();
}
