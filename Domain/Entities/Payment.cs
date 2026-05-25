using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid RepairOrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentMethod Method { get; set; }
    /// <summary>Número de referencia (ej. código SINPE, número de transferencia).</summary>
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;
    public RepairOrder RepairOrder { get; set; } = null!;
}
