using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Common;

public static class InternetOrderCalculator
{
    public static (decimal LinesTotalUsd, decimal LinesTotalCrc, decimal SubtotalCrc, decimal TotalAdvancesCrc, decimal BalanceDueCrc)
        ComputeTotals(
            decimal exchangeRateApplied,
            decimal internationalShippingCost,
            decimal localCourierCost,
            decimal serviceFee,
            IReadOnlyList<InternetOrderLineInputDto> lines,
            IReadOnlyList<InternetOrderAdvanceInputDto> advances)
    {
        if (exchangeRateApplied <= 0)
            throw new ArgumentException("El tipo de cambio debe ser mayor a cero.");

        decimal linesTotalUsd = 0;
        decimal linesTotalCrc = 0;

        foreach (var line in lines)
        {
            if (line.Quantity < 1)
                throw new ArgumentException("Cada línea debe tener cantidad mayor a cero.");
            if (line.UnitPriceUsd < 0)
                throw new ArgumentException("El precio en USD no puede ser negativo.");

            var lineUsd = Math.Round(line.UnitPriceUsd * line.Quantity, 2, MidpointRounding.AwayFromZero);
            var lineCrc = Math.Round(lineUsd * exchangeRateApplied, 2, MidpointRounding.AwayFromZero);
            linesTotalUsd += lineUsd;
            linesTotalCrc += lineCrc;
        }

        var ship = RoundCrc(internationalShippingCost);
        var courier = RoundCrc(localCourierCost);
        var service = RoundCrc(serviceFee);
        var subtotal = RoundCrc(linesTotalCrc + ship + courier + service);

        var totalAdvances = RoundCrc(advances.Sum(a => Math.Max(0, a.AmountCrc)));
        if (totalAdvances > subtotal)
            throw new ArgumentException("La suma de adelantos no puede superar el total del pedido.");

        var balance = RoundCrc(Math.Max(0, subtotal - totalAdvances));

        return (
            RoundUsd(linesTotalUsd),
            RoundCrc(linesTotalCrc),
            subtotal,
            totalAdvances,
            balance);
    }

    public static void ApplyLineSnapshots(
        InternetOrderLineInputDto input,
        decimal exchangeRateApplied,
        out decimal lineTotalUsd,
        out decimal lineTotalCrc)
    {
        lineTotalUsd = Math.Round(input.UnitPriceUsd * input.Quantity, 2, MidpointRounding.AwayFromZero);
        lineTotalCrc = Math.Round(lineTotalUsd * exchangeRateApplied, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal RoundCrc(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);

    private static decimal RoundUsd(decimal value) =>
        Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
