namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>El llamador debe disponer el Stream tras ExecuteAsync.</summary>
public sealed class CatalogVariantImageUploadInput
{
    public required Stream Stream { get; init; }

    public required string ContentType { get; init; }

    public long Length { get; init; }
}
