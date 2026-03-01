namespace MiNegocioCR.Api.Domain.Entities;

public class BusinessSettings
{
    public Guid BusinessId { get; set; }
    public int NextOrderNumber { get; set; } = 1;

    public Business Business { get; set; } = null!;
}