namespace MiNegocioCR.Api.Domain.Entities;

public class CreditCommunication
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid CreditAccountId { get; set; }
    public Guid ContactId { get; set; }

    public int CommunicationType { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CreditAccount CreditAccount { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
}
