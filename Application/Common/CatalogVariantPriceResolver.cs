namespace MiNegocioCR.Api.Application.Common;

/// <summary>
/// Precio de venta persistido: override manual o costo + margen % cuando aplica.
/// </summary>
public static class CatalogVariantPriceResolver
{
    /// <param name="setPriceManually">Si es true, se usa <paramref name="requestedPrice"/>.</param>
    /// <param name="costPrice">Costo unitario (&gt; 0 activa la fórmula junto con margen).</param>
    /// <param name="profitMargin">Margen % de la variante; null no aplica fórmula.</param>
    /// <param name="requestedPrice">Precio enviado por el cliente (manual o fallback).</param>
    public static decimal ResolvePersistedPrice(
        bool setPriceManually,
        decimal costPrice,
        decimal? profitMargin,
        decimal requestedPrice)
    {
        if (setPriceManually)
            return requestedPrice;
        if (costPrice > 0 && profitMargin.HasValue)
            return decimal.Round(costPrice * (1 + profitMargin.Value / 100m), 2, MidpointRounding.AwayFromZero);
        return requestedPrice;
    }
}
