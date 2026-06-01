using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace MiNegocioCR.Api.Application.Common;

public sealed class CampaignImageProcessResult
{
    public required MemoryStream Output { get; init; }
    public string ContentType { get; init; } = "image/jpeg";
    public string FileExtension { get; init; } = ".jpg";
    public int Width { get; init; }
    public int Height { get; init; }
    public long OptimizedBytes { get; init; }
}

/// <summary>
/// Redimensiona y comprime imágenes de campaña para correo (ancho ≤640px, JPEG optimizado).
/// </summary>
public static class CampaignImageProcessor
{
    public static async Task<CampaignImageProcessResult> OptimizeAsync(
        Stream input,
        CancellationToken cancellationToken = default)
    {
        if (input == null || !input.CanRead)
            throw new ArgumentException("Image stream is required.");

        if (input.CanSeek)
            input.Position = 0;

        using var image = await Image.LoadAsync(input, cancellationToken);

        if (image.Width > CampaignImageLimits.MaxDisplayWidth)
        {
            image.Mutate(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(CampaignImageLimits.MaxDisplayWidth, 0),
                Mode = ResizeMode.Max
            }));
        }

        var quality = 85;
        MemoryStream? output = null;
        long bytes = long.MaxValue;

        while (quality >= 55)
        {
            output?.Dispose();
            output = new MemoryStream();
            var encoder = new JpegEncoder { Quality = quality };
            await image.SaveAsJpegAsync(output, encoder, cancellationToken);
            bytes = output.Length;

            if (bytes <= CampaignImageLimits.TargetOptimizedBytes)
                break;

            quality -= 10;
        }

        output!.Position = 0;
        return new CampaignImageProcessResult
        {
            Output = output,
            Width = image.Width,
            Height = image.Height,
            OptimizedBytes = bytes
        };
    }

    public static async Task<CampaignImageProcessResult> OptimizeFromBytesAsync(
        byte[] bytes,
        CancellationToken cancellationToken = default)
    {
        if (bytes.Length == 0)
            throw new ArgumentException("Image file is required.");

        await using var input = new MemoryStream(bytes);
        try
        {
            return await OptimizeAsync(input, cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            throw new ArgumentException("Formato de imagen no reconocido. Usá JPG, PNG, WEBP o GIF.");
        }
    }
}
