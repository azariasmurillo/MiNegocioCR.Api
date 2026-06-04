namespace MiNegocioCR.Api.Domain.Entities;

public class InternetOrderAdvance
{
    public Guid Id { get; set; }
    public Guid InternetOrderId { get; set; }

    public decimal AmountCrc { get; set; }
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;
    public string? Method { get; set; }
    public string? Notes { get; set; }

    public InternetOrder InternetOrder { get; set; } = null!;
}
