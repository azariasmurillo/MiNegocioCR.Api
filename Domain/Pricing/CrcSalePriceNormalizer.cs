namespace MiNegocioCR.Api.Domain.Pricing;

/// <summary>
/// Precios de venta en colones (CRC): valor persistido y expuesto debe ser múltiplo de 5,
/// redondeando hacia arriba cuando no lo sea (alineado con el frontend <c>normalizeColonesSaleUnitPrice</c>).
/// </summary>
public static class CrcSalePriceNormalizer
{
    /// <summary>
    /// 1) Redondeo monetario a 2 decimales (half away from zero).
    /// 2) Si ≤ 0 → 0.
    /// 3) Si ya es múltiplo de 5 → ese valor.
    /// 4) Si no → <c>Ceiling(n / 5) × 5</c>.
    /// </summary>
    public static decimal NormalizeSalePriceColones(decimal amount)
    {
        var n = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        if (n <= 0m)
            return 0m;
        var remainder = n % 5m;
        if (remainder == 0m)
            return n;
        return decimal.Ceiling(n / 5m) * 5m;
    }
}
