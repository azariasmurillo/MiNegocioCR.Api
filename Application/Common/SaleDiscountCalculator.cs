using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Common;

public static class SaleDiscountCalculator
{
    public static SaleDiscountKind ParseKind(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return SaleDiscountKind.None;

        return value.Trim().ToLowerInvariant() switch
        {
            "percent" or "%" => SaleDiscountKind.Percent,
            "fixedamount" or "fixed" or "amount" or "colones" or "₡" => SaleDiscountKind.FixedAmount,
            "none" => SaleDiscountKind.None,
            _ => SaleDiscountKind.None
        };
    }

    /// <summary>
    /// Resuelve kind, valor ingresado y monto aplicado (₡). Usa <paramref name="legacyDiscountColones"/>
    /// como respaldo cuando no hay kind explícito.
    /// </summary>
    public static (SaleDiscountKind Kind, decimal InputValue, decimal Amount) Resolve(
        decimal subtotal,
        string? discountKind,
        decimal discountValue,
        decimal legacyDiscountColones)
    {
        if (subtotal < 0)
            throw new ArgumentException("Subtotal cannot be negative.", nameof(subtotal));
        if (discountValue < 0)
            throw new ArgumentException("Discount value cannot be negative.", nameof(discountValue));
        if (legacyDiscountColones < 0)
            throw new ArgumentException("Discount cannot be negative.", nameof(legacyDiscountColones));

        var kind = ParseKind(discountKind);
        var inputValue = Math.Max(0m, discountValue);
        decimal amount;

        switch (kind)
        {
            case SaleDiscountKind.Percent:
                amount = Math.Round(subtotal * (inputValue / 100m), 2, MidpointRounding.AwayFromZero);
                break;
            case SaleDiscountKind.FixedAmount:
                amount = inputValue;
                break;
            default:
                if (legacyDiscountColones > 0m)
                {
                    kind = SaleDiscountKind.FixedAmount;
                    inputValue = legacyDiscountColones;
                    amount = legacyDiscountColones;
                }
                else
                {
                    kind = SaleDiscountKind.None;
                    inputValue = 0m;
                    amount = 0m;
                }
                break;
        }

        amount = Math.Min(subtotal, Math.Max(0m, amount));
        if (amount <= 0m)
        {
            kind = SaleDiscountKind.None;
            inputValue = 0m;
            amount = 0m;
        }

        return (kind, inputValue, amount);
    }

    public static (decimal TaxAmount, decimal TotalOrden) ComputeTotals(
        decimal subtotal,
        decimal discountAmount,
        decimal taxRatePercent)
    {
        if (taxRatePercent < 0)
            throw new ArgumentException("Tax rate cannot be negative.", nameof(taxRatePercent));

        var taxableBase = subtotal - discountAmount;
        var taxAmount = Math.Round(
            taxableBase * (taxRatePercent / 100m), 2, MidpointRounding.AwayFromZero);
        var totalOrden = taxableBase + taxAmount;
        return (taxAmount, totalOrden);
    }
}
