namespace MiNegocioCR.Api.Application.DTOs;

public class CampaignQuotaDto
{
    public int SentToday { get; set; }
    public int DailyLimit { get; set; }
    public int RemainingToday { get; set; }
    public int SendIntervalSeconds { get; set; } = 1;
}

public class CampaignEligibleContactDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? LastActivityAt { get; set; }
    public DateTime? LastMarketingEmailAt { get; set; }
}

public class CampaignPreviewResultDto
{
    public int InactiveDays { get; set; }
    public int QuietDays { get; set; }
    public string AudienceMode { get; set; } = "Inactive";
    public CampaignQuotaDto Quota { get; set; } = new();
    public List<CampaignEligibleContactDto> EligibleContacts { get; set; } = [];
}

public class SendCampaignEmailRequestDto
{
    public Guid ContactId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public string? ImageUrl { get; set; }
    public int InactiveDays { get; set; } = 60;
    public int QuietDays { get; set; } = 60;
    public string AudienceMode { get; set; } = "Inactive";
}

public class SendCampaignEmailResultDto
{
    public Guid ContactId { get; set; }
    public string Status { get; set; } = "Sent";
    public string? ResendMessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public CampaignQuotaDto Quota { get; set; } = new();
}

public class UploadCampaignImageResultDto
{
    public string ImageUrl { get; set; } = string.Empty;
}

public class QueueCampaignRequestDto
{
    public List<Guid> ContactIds { get; set; } = [];
    public string Subject { get; set; } = string.Empty;
    public string? BodyText { get; set; }
    public string? ImageUrl { get; set; }
    public int InactiveDays { get; set; } = 60;
    public int QuietDays { get; set; } = 60;
    public string AudienceMode { get; set; } = "Inactive";
}

public class QueueCampaignResultDto
{
    public Guid CampaignId { get; set; }
    public string Status { get; set; } = "Queued";
    public int TotalRecipients { get; set; }
    public int QueuePosition { get; set; }
    public int PendingBeforeCampaign { get; set; }
    public DateTime EstimatedStartUtc { get; set; }
    public DateTime EstimatedEndUtc { get; set; }
    public int EstimatedCalendarDays { get; set; }
    public CampaignQuotaDto Quota { get; set; } = new();
}

public class CampaignStatusDto
{
    public Guid CampaignId { get; set; }
    public string Status { get; set; } = "Queued";
    public string SubjectTemplate { get; set; } = string.Empty;
    public int TotalRecipients { get; set; }
    public int SentCount { get; set; }
    public int FailedCount { get; set; }
    public int PendingCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime EstimatedStartUtc { get; set; }
    public DateTime EstimatedEndUtc { get; set; }
    public int QueuePosition { get; set; }
    public CampaignQuotaDto Quota { get; set; } = new();
}
