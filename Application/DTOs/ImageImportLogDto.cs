namespace MiNegocioCR.Api.Application.DTOs;

public sealed class ImageImportLogDto
{
    public string FileName { get; init; } = string.Empty;

    public string? ParsedSku { get; init; }

    public int? SortOrder { get; init; }

    public string Status { get; init; } = string.Empty;

    public string? Message { get; init; }

    public Guid? CatalogVariantId { get; init; }
}
