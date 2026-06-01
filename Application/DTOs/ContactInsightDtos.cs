namespace MiNegocioCR.Api.Application.DTOs;

public class ContactInsightResponseDto
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public int? DaysSinceActivity { get; set; }
    public bool HasEmail { get; set; }
    public bool IsInactive { get; set; }
    public bool HasNeverPaid { get; set; }
    public int PurchaseCount { get; set; }
    public decimal TotalSpent { get; set; }
}

public class ContactInsightsSummaryDto
{
    public int TotalContacts { get; set; }
    public int WithEmail { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int NeverPaidCount { get; set; }
    public int InactiveDaysThreshold { get; set; }
}

public class ContactInsightsResultDto
{
    public ContactInsightsSummaryDto Summary { get; set; } = new();
    public List<ContactInsightResponseDto> Contacts { get; set; } = [];
}

public class ContactActivityItemDto
{
    public string Type { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}

public class ContactActivityResultDto
{
    public Guid ContactId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? LastActivityAt { get; set; }
    public List<ContactActivityItemDto> Items { get; set; } = [];
}
