using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class GetCampaignPreviewUseCase : IGetCampaignPreviewUseCase
{
    private readonly IAppDbContext _context;

    public GetCampaignPreviewUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignPreviewResultDto> Execute(
        Guid businessId,
        int inactiveDays = 60,
        int quietDays = 60,
        CampaignAudienceMode audienceMode = CampaignAudienceMode.Inactive)
    {
        if (inactiveDays < 1)
            inactiveDays = CampaignLimits.DefaultInactiveDays;
        if (quietDays < 1)
            quietDays = CampaignLimits.DefaultQuietDays;

        var utcNow = DateTime.UtcNow;
        var quota = await BuildQuotaAsync(_context, utcNow);

        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var eligible = contacts
            .Where(c => CampaignEligibility.IsEligible(c, inactiveDays, quietDays, utcNow, audienceMode))
            .Select(c => new CampaignEligibleContactDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email!.Trim(),
                LastActivityAt = c.LastActivityAt,
                LastMarketingEmailAt = c.LastMarketingEmailAt
            })
            .ToList();

        return new CampaignPreviewResultDto
        {
            InactiveDays = inactiveDays,
            QuietDays = quietDays,
            AudienceMode = audienceMode.ToString(),
            Quota = quota,
            EligibleContacts = eligible
        };
    }

    internal static async Task<CampaignQuotaDto> BuildQuotaAsync(IAppDbContext context, DateTime utcNow)
    {
        var sentToday = await CampaignQueueMetrics.CountSentTodayGlobalAsync(context, utcNow);
        var dailyLimit = CampaignLimits.PlatformDailyLimit;
        return new CampaignQuotaDto
        {
            SentToday = sentToday,
            DailyLimit = dailyLimit,
            RemainingToday = Math.Max(0, dailyLimit - sentToday),
            SendIntervalSeconds = CampaignLimits.QueueSendIntervalSeconds
        };
    }
}
