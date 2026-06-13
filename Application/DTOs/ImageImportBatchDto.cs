namespace MiNegocioCR.Api.Application.DTOs;

public sealed class ImageImportBatchDto
{
    public Guid Id { get; init; }

    public string Status { get; init; } = string.Empty;

    public int TotalFiles { get; init; }

    public int ProcessedFiles { get; init; }

    public int SuccessCount { get; init; }

    public int SkippedCount { get; init; }

    public int ErrorCount { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? CompletedAt { get; init; }

    public string? SummaryMessage { get; init; }
}
