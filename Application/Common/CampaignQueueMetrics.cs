using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;

namespace MiNegocioCR.Api.Application.Common;

public static class CampaignQueueMetrics
{
    public static async Task<int> CountSentTodayGlobalAsync(IAppDbContext context, DateTime utcNow)
    {
        var todayStart = utcNow.Date;
        return await context.ContactEmailCampaignLogs
            .AsNoTracking()
            .CountAsync(l => l.Status == "Sent" && l.SentAt >= todayStart);
    }

    public static async Task<DateTime?> GetLastSentAtGlobalAsync(IAppDbContext context)
    {
        return await context.ContactEmailCampaignLogs
            .AsNoTracking()
            .Where(l => l.Status == "Sent")
            .OrderByDescending(l => l.SentAt)
            .Select(l => (DateTime?)l.SentAt)
            .FirstOrDefaultAsync();
    }

    public static async Task<int> CountPendingRecipientsAsync(IAppDbContext context)
    {
        return await context.EmailCampaignRecipients
            .AsNoTracking()
            .CountAsync(r => r.Status == "Pending");
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
