using MiNegocioCR.Api.Domain.Pricing;

namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Precio de venta persistido: override manual o costo + margen % + IVA % del negocio;
/// siempre normalizado a CRC (múltiplo de 5 colones hacia arriba).
/// </summary>
public static class CatalogVariantPriceResolver
{
    /// <param name="setPriceManually">Si es true, se usa <paramref name="requestedPrice"/>.</param>
    /// <param name="costPrice">Costo unitario (&gt; 0 activa la fórmula junto con margen).</param>
    /// <param name="profitMargin">Margen % sobre costo (sin IVA); null no aplica fórmula.</param>
    /// <param name="taxRatePercent">IVA % del negocio (0 = sin impuesto en la fórmula).</param>
    /// <param name="requestedPrice">Precio enviado por el cliente (manual o fallback).</param>
    public static decimal ResolvePersistedPrice(
        bool setPriceManually,
        decimal costPrice,
        decimal? profitMargin,
        decimal taxRatePercent,
        decimal requestedPrice)
    {
        decimal raw;
        if (setPriceManually)
            raw = requestedPrice;
        else if (costPrice > 0 && profitMargin.HasValue)
            raw = ComputeGrossSalePrice(costPrice, profitMargin.Value, taxRatePercent);
        else
            raw = requestedPrice;

        return CrcSalePriceNormalizer.NormalizeSalePriceColones(raw);
    }

    /// <summary>
    /// Ganancia sobre costo → subtotal neto → IVA → precio al cliente (IVA incluido).
    /// </summary>
    public static decimal ComputeGrossSalePrice(decimal costPrice, decimal profitMarginPercent, decimal taxRatePercent)
    {
        if (costPrice < 0)
            throw new ArgumentException("CostPrice cannot be negative.", nameof(costPrice));
        if (profitMarginPercent < 0)
            throw new ArgumentException("ProfitMargin cannot be negative.", nameof(profitMarginPercent));
        if (taxRatePercent < 0)
            throw new ArgumentException("TaxRatePercent cannot be negative.", nameof(taxRatePercent));

        var profit = decimal.Round(costPrice * (profitMarginPercent / 100m), 2, MidpointRounding.AwayFromZero);
        var netSubtotal = decimal.Round(costPrice + profit, 2, MidpointRounding.AwayFromZero);
        var tax = taxRatePercent <= 0m
            ? 0m
            : decimal.Round(netSubtotal * (taxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);
        return decimal.Round(netSubtotal + tax, 2, MidpointRounding.AwayFromZero);
    }
}
