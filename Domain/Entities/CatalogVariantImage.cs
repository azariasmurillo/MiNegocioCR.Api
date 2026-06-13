namespace MiNegocioCR.Api.Domain.Entities;

public class CatalogVariantImage
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid CatalogVariantId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? MobileImageUrl { get; set; }

    public string? ThumbnailImageUrl { get; set; }

    public int SortOrder { get; set; }

    public Guid? ImportBatchId { get; set; }

    public string? SourceFileName { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;

    public CatalogVariant CatalogVariant { get; set; } = null!;
}
