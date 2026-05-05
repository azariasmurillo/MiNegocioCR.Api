namespace MiNegocioCR.Api.Application.DTOs;

public sealed class CatalogVariantImageDto
{
    public Guid Id { get; init; }

    public string ImageUrl { get; init; } = string.Empty;

    public bool IsPrimary { get; init; }
}
