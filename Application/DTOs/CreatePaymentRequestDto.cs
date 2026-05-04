using System.Text.Json.Serialization;
using MiNegocioCR.Api.Application.Serialization;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs;

public class CreatePaymentRequestDto
{
    public Guid BusinessId { get; set; }
    public Guid RepairOrderId { get; set; }
    public decimal Amount { get; set; }

    [JsonConverter(typeof(PaymentTypeJsonConverter))]
    public PaymentType Type { get; set; }

    [JsonConverter(typeof(PaymentMethodJsonConverter))]
    public PaymentMethod Method { get; set; }
    public string? Notes { get; set; }
}
