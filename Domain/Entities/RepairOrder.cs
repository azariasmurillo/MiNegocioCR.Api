namespace MiNegocioCR.Api.Domain.Entities;

public class RepairOrder
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public int OrderNumber { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? DeviceDescription { get; set; }
    public string? ProblemDescription { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;
}