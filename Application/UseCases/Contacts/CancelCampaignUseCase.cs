using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class CancelCampaignUseCase : ICancelCampaignUseCase
{
    private readonly IAppDbContext _context;

    public CancelCampaignUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignStatusDto?> Execute(Guid businessId, Guid? campaignId = null)
    {
        var query = _context.EmailCampaigns
            .Where(c => c.BusinessId == businessId && (c.Status == "Queued" || c.Status == "InProgress"));

        if (campaignId.HasValue && campaignId.Value != Guid.Empty)
            query = query.Where(c => c.Id == campaignId.Value);

        var campaign = await query
            .AsTracking()
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();

        if (campaign == null)
            return null;

        var utcNow = DateTime.UtcNow;
        campaign.Status = "Cancelled";
        campaign.CompletedAt = utcNow;

        var pendingRecipients = await _context.EmailCampaignRecipients
            .AsTracking()
            .Where(r => r.CampaignId == campaign.Id
                        && CampaignQueueRecipientStatus.UnfinishedStatuses.Contains(r.Status))
            .ToListAsync();

        foreach (var recipient in pendingRecipients)
        {
            recipient.Status = CampaignQueueRecipientStatus.Cancelled;
            recipient.ProcessedAt = utcNow;
            recipient.ErrorMessage = "Campaña cancelada por el usuario.";
        }

        await _context.SaveChangesAsync(CancellationToken.None);
        await CampaignQueueProgress.SyncCampaignFromRecipientsAsync(_context, campaign.Id);

        return await GetCampaignStatusUseCase.BuildStatusAsync(_context, campaign);
    }
}
