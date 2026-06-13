using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class LocalImageSharpProductImageEnhancerService : IProductImageEnhancerService
{
    public async Task<ProductImageEnhanceResult> EnhanceAsync(
        Stream input,
        ProductImageEnhanceOptions options,
        CancellationToken cancellationToken = default)
    {
        if (options.UseBackgroundRemoval)
        {
            throw new InvalidOperationException(
                "El recorte de fondo aún no está disponible. Desactivá useBackgroundRemoval.");
        }

        using var product = await Image.LoadAsync<Rgba32>(input, cancellationToken);
        await using var mainStream = await RenderToWebpStreamAsync(
            product,
            options.MainSize,
            options.MarketplaceStyle,
            options.WebpQuality,
            cancellationToken);
        await using var mobileStream = await RenderToWebpStreamAsync(
            product,
            options.MobileSize,
            options.MarketplaceStyle,
            options.WebpQuality,
            cancellationToken);
        await using var thumbStream = await RenderToWebpStreamAsync(
            product,
            options.ThumbnailSize,
            options.MarketplaceStyle,
            options.WebpQuality,
            cancellationToken);

        return new ProductImageEnhanceResult
        {
            Main = CloneStream(mainStream),
            Mobile = CloneStream(mobileStream),
            Thumbnail = CloneStream(thumbStream),
        };
    }

    private static async Task<MemoryStream> RenderToWebpStreamAsync(
        Image<Rgba32> source,
        int canvasSize,
        string marketplaceStyle,
        int quality,
        CancellationToken cancellationToken)
    {
        using var canvas = ComposeCanvas(source, canvasSize, marketplaceStyle);
        var output = new MemoryStream();
        var encoder = new WebpEncoder { Quality = quality };
        await canvas.SaveAsWebpAsync(output, encoder, cancellationToken);
        output.Position = 0;
        return output;
    }

    private static Image<Rgba32> ComposeCanvas(Image<Rgba32> source, int canvasSize, string marketplaceStyle)
    {
        var background = MarketplaceStylePresets.ResolveBackground(marketplaceStyle);
        using var product = source.CloneAs<Rgba32>();
        var maxDim = (int)(canvasSize * MarketplaceStylePresets.ProductFillRatio);
        product.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(maxDim, maxDim),
            Mode = ResizeMode.Max,
        }));

        var canvas = new Image<Rgba32>(canvasSize, canvasSize, background);
        var offsetX = (canvasSize - product.Width) / 2;
        var offsetY = (canvasSize - product.Height) / 2;
        canvas.Mutate(ctx => ctx.DrawImage(product, new Point(offsetX, offsetY), 1f));
        return canvas;
    }

    private static MemoryStream CloneStream(MemoryStream source)
    {
        var clone = new MemoryStream();
        if (source.CanSeek)
        {
            source.Position = 0;
        }

        source.CopyTo(clone);
        clone.Position = 0;
        return clone;
    }
}
