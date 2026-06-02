using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;

namespace MiNegocioCR.Api.Application.Common;

public static class CampaignQueueMetrics
{
    public static async Task<int> CountSentTodayGlobalAsync(IAppDbContext context, DateTime utcNow)
    {
        var todayStart = utcNow.Date;

        var fromRecipients = await context.EmailCampaignRecipients
            .AsNoTracking()
            .CountAsync(r => r.Status == CampaignQueueRecipientStatus.Sent
                             && r.ProcessedAt >= todayStart);

        if (fromRecipients > 0)
            return fromRecipients;

        return await context.ContactEmailCampaignLogs
            .AsNoTracking()
            .CountAsync(l => l.Status == "Sent" && l.SentAt >= todayStart);
    }

    public static async Task<DateTime?> GetLastSentAtGlobalAsync(IAppDbContext context)
    {
        var fromRecipients = await context.EmailCampaignRecipients
            .AsNoTracking()
            .Where(r => r.Status == CampaignQueueRecipientStatus.Sent && r.ProcessedAt != null)
            .OrderByDescending(r => r.ProcessedAt)
            .Select(r => r.ProcessedAt)
            .FirstOrDefaultAsync();

        var fromLogs = await context.ContactEmailCampaignLogs
            .AsNoTracking()
            .Where(l => l.Status == "Sent")
            .OrderByDescending(l => l.SentAt)
            .Select(l => (DateTime?)l.SentAt)
            .FirstOrDefaultAsync();

        if (fromRecipients == null)
            return fromLogs;
        if (fromLogs == null)
            return fromRecipients;

        return fromRecipients > fromLogs ? fromRecipients : fromLogs;
    }

    public static async Task<int> CountPendingRecipientsAsync(IAppDbContext context)
    {
        return await context.EmailCampaignRecipients
            .AsNoTracking()
            .CountAsync(r => r.Status == CampaignQueueRecipientStatus.Pending);
    }

    public static async Task<long> GetNextGlobalQueueOrderAsync(IAppDbContext context)
    {
        var max = await context.EmailCampaignRecipients
            .AsNoTracking()
            .Select(r => (long?)r.GlobalQueueOrder)
            .MaxAsync();
        return (max ?? 0) + 1;
    }
}
