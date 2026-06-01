namespace MiNegocioCR.Api.Domain.Entities;

public class EmailCampaign
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string SubjectTemplate { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public string? ImageUrl { get; set; }
    public int InactiveDaysUsed { get; set; }
    public int QuietDaysUsed { get; set; }
    public string AudienceMode { get; set; } = "Inactive";
    /// <summary>Queued | InProgress | Completed | Cancelled</summary>
    public string Status { get; set; } = "Queued";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }

    public Business Business { get; set; } = null!;
    public ICollection<EmailCampaignRecipient> Recipients { get; set; } = [];
}
