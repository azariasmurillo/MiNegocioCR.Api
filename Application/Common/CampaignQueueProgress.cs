using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;

namespace MiNegocioCR.Api.Application.Common;

public sealed class CampaignRecipientProgress
{
    public int SentCount { get; init; }
    public int FailedCount { get; init; }
    public int PendingCount { get; init; }
    public int ProcessingCount { get; init; }
    public int UnfinishedCount => PendingCount + ProcessingCount;
}

public static class CampaignQueueProgress
{
    public static async Task<CampaignRecipientProgress> GetRecipientProgressAsync(
        IAppDbContext context,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var rows = await context.EmailCampaignRecipients
            .AsNoTracking()
            .Where(r => r.CampaignId == campaignId)
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var sent = 0;
        var failed = 0;
        var pending = 0;
        var processing = 0;

        foreach (var row in rows)
        {
            switch (row.Status)
            {
                case CampaignQueueRecipientStatus.Sent:
                    sent = row.Count;
                    break;
                case CampaignQueueRecipientStatus.Failed:
                    failed = row.Count;
                    break;
                case CampaignQueueRecipientStatus.Pending:
                    pending = row.Count;
                    break;
                case CampaignQueueRecipientStatus.Processing:
                    processing = row.Count;
                    break;
            }
        }

        return new CampaignRecipientProgress
        {
            SentCount = sent,
            FailedCount = failed,
            PendingCount = pending,
            ProcessingCount = processing
        };
    }

    /// <summary>
    /// Persiste contadores y estado de campaña desde destinatarios (compatible con NoTracking global).
    /// </summary>
    public static async Task SyncCampaignFromRecipientsAsync(
        IAppDbContext context,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        var campaign = await context.EmailCampaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

        if (campaign == null)
            return;

        var progress = await GetRecipientProgressAsync(context, campaignId, cancellationToken);
        var status = campaign.Status;

        if (status is not ("Cancelled" or "Completed") && progress.UnfinishedCount == 0)
            status = "Completed";

        DateTime? completedAt = campaign.CompletedAt;
        if (status == "Completed" && completedAt == null)
            completedAt = DateTime.UtcNow;

        if (context.Database.IsRelational())
        {
            await context.EmailCampaigns
                .Where(c => c.Id == campaignId)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(c => c.SentCount, progress.SentCount)
                        .SetProperty(c => c.FailedCount, progress.FailedCount)
                        .SetProperty(c => c.Status, status)
                        .SetProperty(c => c.CompletedAt, completedAt),
                    cancellationToken);
            return;
        }

        var tracked = await context.EmailCampaigns
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId, cancellationToken);

        if (tracked == null)
            return;

        tracked.SentCount = progress.SentCount;
        tracked.FailedCount = progress.FailedCount;
        tracked.Status = status;
        tracked.CompletedAt = completedAt;
        await context.SaveChangesAsync(cancellationToken);
    }
}
