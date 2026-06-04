namespace MiNegocioCR.Api.Domain.Entities;

public class InternetOrder
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid ContactId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;
    public int Status { get; set; }

    /// <summary>Tipo de cambio manual (USD→CRC). No exponer al cliente.</summary>
    public decimal ExchangeRateApplied { get; set; }

    public decimal InternationalShippingCost { get; set; }
    public decimal LocalCourierCost { get; set; }
    public decimal ServiceFee { get; set; }

    public decimal LinesTotalUsd { get; set; }
    public decimal LinesTotalCrc { get; set; }
    public decimal SubtotalCrc { get; set; }
    public decimal TotalAdvancesCrc { get; set; }
    public decimal BalanceDueCrc { get; set; }

    public string? CustomerNotes { get; set; }
    public string? InternalNotes { get; set; }
    public string? RefundNote { get; set; }

    public string? ExternalOrderId { get; set; }
    public string? TrackingNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PurchasedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Business Business { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
    public ICollection<InternetOrderLine> Lines { get; set; } = new List<InternetOrderLine>();
    public ICollection<InternetOrderAdvance> Advances { get; set; } = new List<InternetOrderAdvance>();
}
