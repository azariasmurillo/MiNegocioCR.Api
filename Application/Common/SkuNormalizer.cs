using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

public static class SkuNormalizer
{
    public const int MaxLength = 80;

    public static string? NormalizeSku(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"El SKU no puede superar {MaxLength} caracteres.", nameof(raw));
        }

        return trimmed;
    }

    public static string? ToNormalizedKey(string? sku)
    {
        var normalized = NormalizeSku(sku);
        return normalized?.ToLowerInvariant();
    }

    public static void Apply(CatalogVariant variant, Guid businessId, string? rawSku)
    {
        variant.BusinessId = businessId;
        variant.SKU = NormalizeSku(rawSku);
        variant.SkuNormalized = ToNormalizedKey(variant.SKU);
    }
}
