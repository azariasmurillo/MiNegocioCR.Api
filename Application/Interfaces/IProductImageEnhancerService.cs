using MiNegocioCR.Api.Application.Common;

namespace MiNegocioCR.Api.Application.Interfaces;

public interface IProductImageEnhancerService
{
    Task<ProductImageEnhanceResult> EnhanceAsync(
        Stream input,
        ProductImageEnhanceOptions options,
        CancellationToken cancellationToken = default);
}

public sealed class ProductImageEnhanceOptions
{
    public string MarketplaceStyle { get; init; } = MarketplaceStylePresets.WhiteV1;

    public bool UseBackgroundRemoval { get; init; }

    public int MainSize { get; init; } = 1200;

    public int MobileSize { get; init; } = 600;

    public int ThumbnailSize { get; init; } = 300;

    public int WebpQuality { get; init; } = 88;
}

public sealed class ProductImageEnhanceResult
{
    public required MemoryStream Main { get; init; }

    public required MemoryStream Mobile { get; init; }

    public required MemoryStream Thumbnail { get; init; }
}
