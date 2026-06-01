namespace MiNegocioCR.Api.Domain.Entities;

public class EmailCampaignRecipient
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid ContactId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    /// <summary>Pending | Sent | Failed | Skipped</summary>
    public string Status { get; set; } = "Pending";
    public long GlobalQueueOrder { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResendMessageId { get; set; }

    public EmailCampaign Campaign { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
}
