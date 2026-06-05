namespace MiNegocioCR.Api.Domain.Entities;

public class CreditTransaction
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid CreditAccountId { get; set; }
    public Guid ContactId { get; set; }

    public int TransactionType { get; set; }
    public decimal AmountCrc { get; set; }
    public decimal? AppliedToBalanceCrc { get; set; }
    public decimal? ChangeGivenCrc { get; set; }
    public string? Description { get; set; }
    public decimal PreviousBalanceCrc { get; set; }
    public decimal NewBalanceCrc { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CreditAccount CreditAccount { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
    public ICollection<CreditTransactionLine> Lines { get; set; } = new List<CreditTransactionLine>();
}
