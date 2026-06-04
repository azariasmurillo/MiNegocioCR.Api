using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

public static class InternetOrderPersistenceHelper
{
    public static void ValidateUpsertRequest(UpsertInternetOrderRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.Lines == null || request.Lines.Count == 0)
            throw new ArgumentException("Agregá al menos una línea de producto.");
        if (request.ExchangeRateApplied <= 0)
            throw new ArgumentException("El tipo de cambio debe ser mayor a cero.");

        foreach (var line in request.Lines)
        {
            if (string.IsNullOrWhiteSpace(line.ProductName))
                throw new ArgumentException("Cada línea requiere nombre.");
            if (string.IsNullOrWhiteSpace(line.ProductUrl))
                throw new ArgumentException("Cada línea requiere link del producto.");
            if (!Uri.TryCreate(line.ProductUrl.Trim(), UriKind.Absolute, out var uri)
                || uri.Scheme is not "http" and not "https")
                throw new ArgumentException("El link del producto debe ser una URL válida (http/https).");
        }

        if (!request.ContactId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
                throw new ArgumentException("Se requiere el nombre del cliente o un ContactId.");
            if (string.IsNullOrWhiteSpace(PhoneSanitizer.Sanitize(request.CustomerPhone)))
                throw new ArgumentException("Se requiere el teléfono del cliente o un ContactId.");
        }
    }

    public static void ApplyTotalsToOrder(
        InternetOrder order,
        decimal exchangeRate,
        decimal internationalShipping,
        decimal localCourier,
        decimal serviceFee,
        IReadOnlyList<InternetOrderLineInputDto> lineInputs,
        IReadOnlyList<InternetOrderAdvanceInputDto> advanceInputs)
    {
        var totals = InternetOrderCalculator.ComputeTotals(
            exchangeRate,
            internationalShipping,
            localCourier,
            serviceFee,
            lineInputs,
            advanceInputs);

        order.ExchangeRateApplied = exchangeRate;
        order.InternationalShippingCost = Math.Round(internationalShipping, 2, MidpointRounding.AwayFromZero);
        order.LocalCourierCost = Math.Round(localCourier, 2, MidpointRounding.AwayFromZero);
        order.ServiceFee = Math.Round(serviceFee, 2, MidpointRounding.AwayFromZero);
        order.LinesTotalUsd = totals.LinesTotalUsd;
        order.LinesTotalCrc = totals.LinesTotalCrc;
        order.SubtotalCrc = totals.SubtotalCrc;
        order.TotalAdvancesCrc = totals.TotalAdvancesCrc;
        order.BalanceDueCrc = totals.BalanceDueCrc;
        order.UpdatedAt = DateTime.UtcNow;
    }

    public static void ApplyMetadataFromRequest(InternetOrder order, UpsertInternetOrderRequestDto request)
    {
        order.CustomerNotes = string.IsNullOrWhiteSpace(request.CustomerNotes) ? null : request.CustomerNotes.Trim();
        order.InternalNotes = string.IsNullOrWhiteSpace(request.InternalNotes) ? null : request.InternalNotes.Trim();
        order.RefundNote = string.IsNullOrWhiteSpace(request.RefundNote) ? null : request.RefundNote.Trim();
        order.ExternalOrderId = string.IsNullOrWhiteSpace(request.ExternalOrderId) ? null : request.ExternalOrderId.Trim();
        order.TrackingNumber = string.IsNullOrWhiteSpace(request.TrackingNumber) ? null : request.TrackingNumber.Trim();
    }

    public static List<InternetOrderLine> BuildLines(
        Guid orderId,
        decimal exchangeRate,
        IReadOnlyList<InternetOrderLineInputDto> inputs)
    {
        var list = new List<InternetOrderLine>();
        for (var i = 0; i < inputs.Count; i++)
        {
            var input = inputs[i];
            InternetOrderCalculator.ApplyLineSnapshots(input, exchangeRate, out var lineUsd, out var lineCrc);
            list.Add(new InternetOrderLine
            {
                Id = Guid.NewGuid(),
                InternetOrderId = orderId,
                SortOrder = i,
                ProductName = input.ProductName.Trim(),
                ProductUrl = input.ProductUrl.Trim(),
                UnitPriceUsd = input.UnitPriceUsd,
                Quantity = input.Quantity,
                LineTotalUsd = lineUsd,
                LineTotalCrc = lineCrc
            });
        }

        return list;
    }

    public static List<InternetOrderAdvance> BuildAdvances(
        Guid orderId,
        IReadOnlyList<InternetOrderAdvanceInputDto> inputs)
    {
        return inputs
            .Where(a => a.AmountCrc > 0)
            .Select(a => new InternetOrderAdvance
            {
                Id = Guid.NewGuid(),
                InternetOrderId = orderId,
                AmountCrc = Math.Round(a.AmountCrc, 2, MidpointRounding.AwayFromZero),
                PaidAt = a.PaidAt ?? DateTime.UtcNow,
                Method = string.IsNullOrWhiteSpace(a.Method) ? null : a.Method.Trim(),
                Notes = string.IsNullOrWhiteSpace(a.Notes) ? null : a.Notes.Trim()
            })
            .ToList();
    }
}
