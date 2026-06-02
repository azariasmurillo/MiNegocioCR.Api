using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class QueueCampaignUseCase : IQueueCampaignUseCase
{
    private readonly IAppDbContext _context;

    public QueueCampaignUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<QueueCampaignResultDto> Execute(Guid businessId, QueueCampaignRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new ArgumentException("Subject is required.", nameof(request.Subject));
        if (request.ContactIds == null || request.ContactIds.Count == 0)
            throw new ArgumentException("At least one contact is required.");

        CampaignContentValidator.Validate(request.Subject, request.BodyText, request.ImageUrl);

        var inactiveDays = request.InactiveDays < 1 ? CampaignLimits.DefaultInactiveDays : request.InactiveDays;
        var quietDays = request.QuietDays < 1 ? CampaignLimits.DefaultQuietDays : request.QuietDays;
        var audienceMode = CampaignAudienceModeParser.Parse(request.AudienceMode);
        var utcNow = DateTime.UtcNow;

        var business = await _context.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId)
            ?? throw new ArgumentException("Business not found.");

        if (!business.EnableEmailNotifications)
            throw new InvalidOperationException("Las notificaciones por correo están desactivadas para este negocio.");

        var hasActive = await _context.EmailCampaigns
            .AsNoTracking()
            .AnyAsync(c => c.BusinessId == businessId && (c.Status == "Queued" || c.Status == "InProgress"));
        if (hasActive)
            throw new InvalidOperationException("Ya tenés una campaña en curso. Esperá a que termine antes de encolar otra.");

        var distinctIds = request.ContactIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var contacts = await _context.Contacts
            .AsNoTracking()
            .Where(c => c.BusinessId == businessId && !c.IsDeleted && distinctIds.Contains(c.Id))
            .ToListAsync();

        var eligible = contacts
            .Where(c => CampaignEligibility.IsEligible(c, inactiveDays, quietDays, utcNow, audienceMode))
            .OrderBy(c => c.Name)
            .ToList();

        eligible = CampaignRecipientDeduplication.ByEmail(eligible);

        if (eligible.Count == 0)
            throw new InvalidOperationException("Ningún contacto seleccionado es elegible para esta campaña.");

        var pendingBefore = await CampaignQueueMetrics.CountPendingRecipientsAsync(_context);
        var nextOrder = await CampaignQueueMetrics.GetNextGlobalQueueOrderAsync(_context);

        var campaign = new EmailCampaign
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            SubjectTemplate = request.Subject.Trim(),
            BodyText = string.IsNullOrWhiteSpace(request.BodyText) ? null : request.BodyText.Trim(),
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            InactiveDaysUsed = inactiveDays,
            QuietDaysUsed = quietDays,
            AudienceMode = audienceMode.ToString(),
            Status = "Queued",
            CreatedAt = utcNow,
            TotalRecipients = eligible.Count
        };

        var recipients = new List<EmailCampaignRecipient>();
        for (var i = 0; i < eligible.Count; i++)
        {
            var contact = eligible[i];
            recipients.Add(new EmailCampaignRecipient
            {
                Id = Guid.NewGuid(),
                CampaignId = campaign.Id,
                ContactId = contact.Id,
                ContactName = contact.Name ?? string.Empty,
                ContactEmail = contact.Email!.Trim(),
                Status = "Pending",
                GlobalQueueOrder = nextOrder + i
            });
        }

        _context.EmailCampaigns.Add(campaign);
        _context.EmailCampaignRecipients.AddRange(recipients);
        await _context.SaveChangesAsync(CancellationToken.None);

        var sentToday = await CampaignQueueMetrics.CountSentTodayGlobalAsync(_context, utcNow);
        var estimate = CampaignQueueEstimator.Estimate(
            utcNow,
            pendingBefore,
            eligible.Count,
            sentToday,
            CampaignLimits.PlatformDailyLimit,
            CampaignLimits.QueueSendIntervalSeconds);

        var quota = await GetCampaignPreviewUseCase.BuildQuotaAsync(_context, utcNow);

        return new QueueCampaignResultDto
        {
            CampaignId = campaign.Id,
            Status = campaign.Status,
            TotalRecipients = campaign.TotalRecipients,
            QueuePosition = estimate.QueuePosition,
            PendingBeforeCampaign = estimate.PendingBeforeCampaign,
            EstimatedStartUtc = estimate.EstimatedStartUtc,
            EstimatedEndUtc = estimate.EstimatedEndUtc,
            EstimatedCalendarDays = estimate.EstimatedCalendarDays,
            Quota = quota
        };
    }
}
