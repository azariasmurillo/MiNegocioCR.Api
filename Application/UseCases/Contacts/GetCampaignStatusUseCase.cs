using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class GetCampaignStatusUseCase : IGetCampaignStatusUseCase
{
    private readonly IAppDbContext _context;

    public GetCampaignStatusUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignStatusDto?> Execute(Guid businessId, Guid campaignId)
    {
        var campaign = await _context.EmailCampaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == campaignId && c.BusinessId == businessId);

        if (campaign == null)
            return null;

        return await BuildStatusAsync(_context, campaign);
    }

    internal static async Task<CampaignStatusDto> BuildStatusAsync(IAppDbContext context, Domain.Entities.EmailCampaign campaign)
    {
        var utcNow = DateTime.UtcNow;
        var pendingCount = await context.EmailCampaignRecipients
            .AsNoTracking()
            .CountAsync(r => r.CampaignId == campaign.Id && r.Status == "Pending");

        var firstPendingOrder = await context.EmailCampaignRecipients
            .AsNoTracking()
            .Where(r => r.CampaignId == campaign.Id && r.Status == "Pending")
            .OrderBy(r => r.GlobalQueueOrder)
            .Select(r => (long?)r.GlobalQueueOrder)
            .FirstOrDefaultAsync();

        var pendingBefore = 0;
        if (firstPendingOrder.HasValue)
        {
            pendingBefore = await context.EmailCampaignRecipients
                .AsNoTracking()
                .CountAsync(r => r.Status == "Pending" && r.GlobalQueueOrder < firstPendingOrder.Value);
        }

        var sentToday = await CampaignQueueMetrics.CountSentTodayGlobalAsync(context, utcNow);
        var remainingInCampaign = pendingCount;
        var estimate = CampaignQueueEstimator.Estimate(
            utcNow,
            pendingBefore,
            Math.Max(remainingInCampaign, 1),
            sentToday,
            CampaignLimits.PlatformDailyLimit,
            CampaignLimits.QueueSendIntervalSeconds);

        var quota = await GetCampaignPreviewUseCase.BuildQuotaAsync(context, utcNow);

        return new CampaignStatusDto
        {
            CampaignId = campaign.Id,
            Status = campaign.Status,
            SubjectTemplate = campaign.SubjectTemplate,
            TotalRecipients = campaign.TotalRecipients,
            SentCount = campaign.SentCount,
            FailedCount = campaign.FailedCount,
            PendingCount = pendingCount,
            CreatedAt = campaign.CreatedAt,
            CompletedAt = campaign.CompletedAt,
            EstimatedStartUtc = estimate.EstimatedStartUtc,
            EstimatedEndUtc = estimate.EstimatedEndUtc,
            QueuePosition = estimate.QueuePosition,
            Quota = quota
        };
    }
}
