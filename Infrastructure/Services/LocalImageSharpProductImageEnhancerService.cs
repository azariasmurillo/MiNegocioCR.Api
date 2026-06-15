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
        using var product = source.CloneAs<Rgba32>();
        var maxDim = (int)(canvasSize * MarketplaceStylePresets.ProductFillRatio);
        product.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(maxDim, maxDim),
            Mode = ResizeMode.Max,
        }));

        var canvas = CreateBackground(canvasSize, marketplaceStyle);
        var offsetX = (canvasSize - product.Width) / 2;
        var offsetY = (canvasSize - product.Height) / 2;

        DrawContactShadow(canvas, product.Width, product.Height, offsetX, offsetY);
        canvas.Mutate(ctx => ctx.DrawImage(product, new Point(offsetX, offsetY), 1f));
        return canvas;
    }

    private static Image<Rgba32> CreateBackground(int canvasSize, string marketplaceStyle)
    {
        if (!string.Equals(marketplaceStyle, MarketplaceStylePresets.SoftV1, StringComparison.OrdinalIgnoreCase))
        {
            return new Image<Rgba32>(canvasSize, canvasSize, MarketplaceStylePresets.ResolveBackground(marketplaceStyle));
        }

        var top = new Rgba32(247, 249, 251, 255);
        var bottom = new Rgba32(238, 242, 246, 255);
        var canvas = new Image<Rgba32>(canvasSize, canvasSize);
        for (var y = 0; y < canvasSize; y++)
        {
            var t = y / (float)(canvasSize - 1);
            var r = (byte)(top.R + (bottom.R - top.R) * t);
            var g = (byte)(top.G + (bottom.G - top.G) * t);
            var b = (byte)(top.B + (bottom.B - top.B) * t);
            var rowColor = new Rgba32(r, g, b, 255);
            for (var x = 0; x < canvasSize; x++)
            {
                canvas[x, y] = rowColor;
            }
        }

        return canvas;
    }

    private static void DrawContactShadow(
        Image<Rgba32> canvas,
        int productWidth,
        int productHeight,
        int offsetX,
        int offsetY)
    {
        var shadowWidth = Math.Max(32, (int)(productWidth * 0.55f));
        var shadowHeight = Math.Max(16, (int)(productHeight * 0.07f));
        var shadowX = offsetX + (productWidth - shadowWidth) / 2;
        var shadowY = offsetY + productHeight - shadowHeight / 2;

        using var shadow = new Image<Rgba32>(shadowWidth, shadowHeight, Color.Transparent);
        var centerX = shadowWidth / 2f;
        var centerY = shadowHeight / 2f;
        var maxAlpha = (byte)(255 * MarketplaceStylePresets.ContactShadowOpacity);

        for (var y = 0; y < shadowHeight; y++)
        {
            for (var x = 0; x < shadowWidth; x++)
            {
                var dx = (x - centerX) / centerX;
                var dy = (y - centerY) / centerY;
                var distance = dx * dx + dy * dy;
                if (distance > 1f)
                {
                    continue;
                }

                var falloff = 1f - distance;
                shadow[x, y] = new Rgba32(0, 0, 0, (byte)(maxAlpha * falloff));
            }
        }

        canvas.Mutate(ctx => ctx.DrawImage(shadow, new Point(shadowX, shadowY), 1f));
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
