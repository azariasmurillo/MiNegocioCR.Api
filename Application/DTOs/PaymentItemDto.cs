using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs;

public class PaymentItemDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime CreatedAt { get; set; }
}
