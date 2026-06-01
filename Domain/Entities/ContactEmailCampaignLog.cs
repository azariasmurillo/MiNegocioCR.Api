namespace MiNegocioCR.Api.Domain.Entities;

public class ContactEmailCampaignLog
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Guid ContactId { get; set; }
    public DateTime SentAt { get; set; }
    public string Subject { get; set; } = string.Empty;
    /// <summary>Sent | Failed</summary>
    public string Status { get; set; } = "Sent";
    public string? ResendMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public int InactiveDaysUsed { get; set; }
    public int QuietDaysUsed { get; set; }

    public Business Business { get; set; } = null!;
    public Contact Contact { get; set; } = null!;
}
