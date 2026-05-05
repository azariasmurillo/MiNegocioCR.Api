namespace MiNegocioCR.Api.Domain.Entities;

public class RepairOrderImage
{
    public Guid Id { get; set; }

    public Guid BusinessId { get; set; }

    public Guid RepairOrderId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;

    public RepairOrder RepairOrder { get; set; } = null!;
}
