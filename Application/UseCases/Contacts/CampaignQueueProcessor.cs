using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class CampaignQueueProcessor : ICampaignQueueProcessor
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<CampaignQueueProcessor> _logger;

    public CampaignQueueProcessor(
        IAppDbContext context,
        IEmailService emailService,
        ILogger<CampaignQueueProcessor> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        var sentToday = await CampaignQueueMetrics.CountSentTodayGlobalAsync(_context, utcNow);
        if (sentToday >= CampaignLimits.PlatformDailyLimit)
        {
            _logger.LogDebug("Campaign queue: daily platform limit reached ({SentToday}).", sentToday);
            return false;
        }

        var lastSentAt = await CampaignQueueMetrics.GetLastSentAtGlobalAsync(_context);
        if (lastSentAt.HasValue)
        {
            var elapsed = utcNow - lastSentAt.Value;
            if (elapsed.TotalSeconds < CampaignLimits.QueueSendIntervalSeconds)
                return false;
        }

        var recipient = await _context.EmailCampaignRecipients
            .Include(r => r.Campaign)
            .Where(r => r.Status == "Pending"
                        && (r.Campaign.Status == "Queued" || r.Campaign.Status == "InProgress"))
            .OrderBy(r => r.GlobalQueueOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (recipient == null)
            return false;

        var campaign = recipient.Campaign;
        var business = await _context.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == campaign.BusinessId, cancellationToken);

        if (business == null || !business.EnableEmailNotifications)
        {
            recipient.Status = "Skipped";
            recipient.ProcessedAt = utcNow;
            recipient.ErrorMessage = "Negocio no disponible o correo desactivado.";
            await UpdateCampaignProgressAsync(campaign, utcNow, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        var contact = await _context.Contacts
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == recipient.ContactId && c.BusinessId == campaign.BusinessId && !c.IsDeleted, cancellationToken);

        if (contact == null || !CampaignEligibility.HasValidEmail(contact))
        {
            recipient.Status = "Skipped";
            recipient.ProcessedAt = utcNow;
            recipient.ErrorMessage = "Contacto sin correo válido.";
            await UpdateCampaignProgressAsync(campaign, utcNow, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        if (campaign.Status == "Queued")
            campaign.Status = "InProgress";

        var subject = CampaignPersonalization.ApplySubjectTemplate(campaign.SubjectTemplate, recipient.ContactName);
        var html = CampaignEmailHtmlBuilder.Build(
            business.Name,
            business.LogoUrl,
            campaign.BodyText,
            campaign.ImageUrl,
            recipient.ContactName);

        var log = new ContactEmailCampaignLog
        {
            Id = Guid.NewGuid(),
            BusinessId = campaign.BusinessId,
            ContactId = contact.Id,
            SentAt = utcNow,
            Subject = subject,
            InactiveDaysUsed = campaign.InactiveDaysUsed,
            QuietDaysUsed = campaign.QuietDaysUsed
        };

        try
        {
            var messageId = await _emailService.SendCampaignAsync(
                business,
                recipient.ContactEmail,
                subject,
                html,
                cancellationToken);

            recipient.Status = "Sent";
            recipient.ProcessedAt = utcNow;
            recipient.ResendMessageId = messageId;
            log.Status = "Sent";
            log.ResendMessageId = messageId;
            contact.LastMarketingEmailAt = utcNow;
            campaign.SentCount++;
        }
        catch (Exception ex)
        {
            var message = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
            recipient.Status = "Failed";
            recipient.ProcessedAt = utcNow;
            recipient.ErrorMessage = message;
            log.Status = "Failed";
            log.ErrorMessage = message;
            campaign.FailedCount++;
            _logger.LogWarning(ex, "Campaign queue send failed. CampaignId={CampaignId}, ContactId={ContactId}", campaign.Id, contact.Id);
        }

        _context.ContactEmailCampaignLogs.Add(log);
        await UpdateCampaignProgressAsync(campaign, utcNow, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Campaign queue processed recipient {RecipientId}. Status={Status}, CampaignId={CampaignId}",
            recipient.Id,
            recipient.Status,
            campaign.Id);

        return true;
    }

    private async Task UpdateCampaignProgressAsync(EmailCampaign campaign, DateTime utcNow, CancellationToken cancellationToken)
    {
        var pending = await _context.EmailCampaignRecipients
            .CountAsync(r => r.CampaignId == campaign.Id && r.Status == "Pending", cancellationToken);

        if (pending == 0)
        {
            campaign.Status = "Completed";
            campaign.CompletedAt = utcNow;
        }
    }
}
