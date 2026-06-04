using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs;

public class InternetOrderLineInputDto
{
    public Guid? Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductUrl { get; set; } = string.Empty;
    public decimal UnitPriceUsd { get; set; }
    public int Quantity { get; set; } = 1;
}

public class InternetOrderAdvanceInputDto
{
    public Guid? Id { get; set; }
    public decimal AmountCrc { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Method { get; set; }
    public string? Notes { get; set; }
}

public class UpsertInternetOrderRequestDto
{
    public Guid? ContactId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }

    public decimal ExchangeRateApplied { get; set; }
    public decimal InternationalShippingCost { get; set; }
    public decimal LocalCourierCost { get; set; }
    public decimal ServiceFee { get; set; }

    public string? CustomerNotes { get; set; }
    public string? InternalNotes { get; set; }
    public string? RefundNote { get; set; }
    public string? ExternalOrderId { get; set; }
    public string? TrackingNumber { get; set; }

    public List<InternetOrderLineInputDto> Lines { get; set; } = new();
    public List<InternetOrderAdvanceInputDto> Advances { get; set; } = new();
}

public class UpdateInternetOrderStatusRequestDto
{
    public InternetOrderStatus NewStatus { get; set; }
    public string? RefundNote { get; set; }
}
