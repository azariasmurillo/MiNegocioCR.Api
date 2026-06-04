using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Common;

public static class InternetOrderProjection
{
    public static object MapDetail(InternetOrder order, bool includeExchangeRate)
    {
        var status = (InternetOrderStatus)order.Status;
        var lines = order.Lines
            .OrderBy(l => l.SortOrder)
            .ThenBy(l => l.Id)
            .Select(l => new
            {
                l.Id,
                l.SortOrder,
                l.ProductName,
                l.ProductUrl,
                l.UnitPriceUsd,
                l.Quantity,
                l.LineTotalUsd,
                l.LineTotalCrc
            })
            .ToList();

        var advances = order.Advances
            .OrderBy(a => a.PaidAt)
            .ThenBy(a => a.Id)
            .Select(a => new
            {
                a.Id,
                a.AmountCrc,
                a.PaidAt,
                a.Method,
                a.Notes
            })
            .ToList();

        return new
        {
            order.Id,
            order.OrderNumber,
            order.BusinessId,
            order.ContactId,
            Contact = new
            {
                order.Contact.Id,
                Name = order.Contact.Name,
                Phone = order.Contact.Phone,
                Email = order.Contact.Email
            },
            Status = status.ToString(),
            ExchangeRateApplied = includeExchangeRate ? order.ExchangeRateApplied : (decimal?)null,
            order.InternationalShippingCost,
            order.LocalCourierCost,
            order.ServiceFee,
            order.LinesTotalUsd,
            order.LinesTotalCrc,
            order.SubtotalCrc,
            order.TotalAdvancesCrc,
            order.BalanceDueCrc,
            order.CustomerNotes,
            order.InternalNotes,
            order.RefundNote,
            order.ExternalOrderId,
            order.TrackingNumber,
            order.CreatedAt,
            order.UpdatedAt,
            order.PurchasedAt,
            order.ReceivedAt,
            order.DeliveredAt,
            order.CancelledAt,
            Lines = lines,
            Advances = advances,
            LinesSubtotalUsd = order.LinesTotalUsd
        };
    }

    public static object MapSummary(InternetOrder order) => new
    {
        order.Id,
        order.OrderNumber,
        order.ContactId,
        Contact = new
        {
            order.Contact.Id,
            Name = order.Contact.Name,
            Phone = order.Contact.Phone,
            Email = order.Contact.Email
        },
        Status = ((InternetOrderStatus)order.Status).ToString(),
        order.SubtotalCrc,
        order.TotalAdvancesCrc,
        order.BalanceDueCrc,
        order.LinesTotalUsd,
        LineCount = order.Lines.Count,
        order.CreatedAt
    };
}
