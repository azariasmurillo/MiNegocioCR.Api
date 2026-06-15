using SixLabors.ImageSharp.PixelFormats;

namespace MiNegocioCR.Api.Application.Common;

public static class MarketplaceStylePresets
{
    public const string WhiteV1 = "marketplace-white-v1";
    public const string SoftV1 = "marketplace-soft-v1";

    public static Rgba32 ResolveBackground(string? style)
    {
        return string.Equals(style, SoftV1, StringComparison.OrdinalIgnoreCase)
            ? new Rgba32(247, 249, 251, 255)
            : new Rgba32(255, 255, 255, 255);
    }

    public static float ProductFillRatio => 0.78f;

    public static float ContactShadowOpacity => 0.07f;
}
