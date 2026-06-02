namespace MiNegocioCR.Api.Application.Common;

public static class CampaignQueueRecipientStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
    public const string Skipped = "Skipped";
    public const string Cancelled = "Cancelled";

    public static readonly string[] ActiveQueueStatuses = [Pending, Processing];

    public static readonly string[] UnfinishedStatuses = [Pending, Processing];
}
