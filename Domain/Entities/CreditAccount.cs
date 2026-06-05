namespace MiNegocioCR.Api.Domain.Entities;

public class CreditAccount
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid ContactId { get; set; }

    public string AccountNumber { get; set; } = string.Empty;
    public int Status { get; set; }

    public decimal CurrentBalanceCrc { get; set; }
    public decimal TotalChargedCrc { get; set; }

    public DateTime? PaymentCommitmentDate { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public Business Business { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
    public ICollection<CreditTransaction> Transactions { get; set; } = new List<CreditTransaction>();
    public ICollection<CreditCommunication> Communications { get; set; } = new List<CreditCommunication>();
}
