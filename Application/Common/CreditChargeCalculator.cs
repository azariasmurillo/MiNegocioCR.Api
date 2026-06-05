using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Common;

public static class CreditChargeCalculator
{
    public static decimal RoundCrc(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    public static (List<ResolvedCreditChargeLineDto> Lines, decimal TotalCrc) ResolveLines(
        IReadOnlyList<CreditChargeLineInputDto> inputs)
    {
        if (inputs == null || inputs.Count == 0)
            throw new ArgumentException("Agregá al menos una línea al cargo.");

        var resolved = new List<ResolvedCreditChargeLineDto>();
        decimal total = 0;

        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            var kind = ParseLineKind(input.LineKind);
            var qty = input.Quantity < 1 ? 1 : input.Quantity;
            var concept = (input.ConceptName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(concept))
                throw new ArgumentException($"Línea {i + 1}: indicá el nombre del producto o concepto.");

            decimal basePrice = RoundCrc(Math.Max(0, input.BaseUnitPriceCrc));
            var markup = Math.Max(0, input.CreditMarkupPercent);
            var unitPrice = input.UnitPriceCrc > 0
                ? RoundCrc(input.UnitPriceCrc)
                : RoundCrc(basePrice * (1 + markup / 100m));
            var lineTotal = RoundCrc(unitPrice * qty);

            if (kind == CreditTransactionLineKind.Inventory)
            {
                if (!input.CatalogVariantId.HasValue || input.CatalogVariantId == Guid.Empty)
                    throw new ArgumentException($"Línea {i + 1}: seleccioná una variante de inventario.");
            }
            else if (input.CatalogVariantId.HasValue)
            {
                throw new ArgumentException($"Línea {i + 1}: los conceptos libres no llevan variante de inventario.");
            }

            resolved.Add(new ResolvedCreditChargeLineDto
            {
                SortOrder = i,
                LineKind = kind,
                CatalogVariantId = kind == CreditTransactionLineKind.Inventory ? input.CatalogVariantId : null,
                ConceptName = concept,
                Quantity = qty,
                BaseUnitPriceCrc = basePrice,
                CreditMarkupPercent = markup,
                UnitPriceCrc = unitPrice,
                LineTotalCrc = lineTotal,
            });
            total += lineTotal;
        }

        return (resolved, RoundCrc(total));
    }

    public static (decimal Applied, decimal ChangeGiven, decimal NewBalance) ApplyPayment(
        decimal currentBalance,
        decimal paymentAmount)
    {
        if (paymentAmount <= 0)
            throw new ArgumentException("El monto del abono debe ser mayor a cero.");

        var balance = RoundCrc(Math.Max(0, currentBalance));
        var paid = RoundCrc(paymentAmount);
        var applied = RoundCrc(Math.Min(balance, paid));
        var change = RoundCrc(Math.Max(0, paid - balance));
        var newBalance = RoundCrc(Math.Max(0, balance - paid));
        return (applied, change, newBalance);
    }

    private static CreditTransactionLineKind ParseLineKind(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return CreditTransactionLineKind.FreeConcept;

        var v = raw.Trim();
        if (v.Equals("Inventory", StringComparison.OrdinalIgnoreCase)
            || v.Equals("Inventario", StringComparison.OrdinalIgnoreCase))
            return CreditTransactionLineKind.Inventory;

        return CreditTransactionLineKind.FreeConcept;
    }
}
