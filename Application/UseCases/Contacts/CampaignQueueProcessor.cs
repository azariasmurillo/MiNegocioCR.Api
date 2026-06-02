using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Domain.Entities;
using ContactEntity = MiNegocioCR.Api.Domain.Entities.Contact;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class CampaignQueueProcessor : ICampaignQueueProcessor
{
    private static readonly TimeSpan StaleProcessingThreshold = TimeSpan.FromMinutes(15);

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
        await RecoverStaleProcessingRecipientsAsync(utcNow, cancellationToken);

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

        var recipientId = await TryClaimNextRecipientIdAsync(utcNow, cancellationToken);
        if (recipientId == null)
            return false;

        var recipient = await _context.EmailCampaignRecipients
            .AsTracking()
            .Include(r => r.Campaign)
            .FirstOrDefaultAsync(r => r.Id == recipientId.Value, cancellationToken);

        if (recipient == null || recipient.Status != CampaignQueueRecipientStatus.Processing)
            return false;

        var campaign = recipient.Campaign;
        if (campaign.Status is "Cancelled" or "Completed")
        {
            await MarkRecipientAsync(
                recipient.Id,
                CampaignQueueRecipientStatus.Cancelled,
                utcNow,
                "Campaña ya no está activa.",
                cancellationToken);
            return true;
        }

        var business = await _context.Businesses
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == campaign.BusinessId, cancellationToken);

        if (business == null || !business.EnableEmailNotifications)
        {
            await FinalizeRecipientAsync(
                recipient,
                campaign,
                CampaignQueueRecipientStatus.Skipped,
                utcNow,
                errorMessage: "Negocio no disponible o correo desactivado.",
                resendMessageId: null,
                contact: null,
                subject: null,
                cancellationToken: cancellationToken);
            return true;
        }

        var contact = await _context.Contacts
            .AsTracking()
            .FirstOrDefaultAsync(
                c => c.Id == recipient.ContactId && c.BusinessId == campaign.BusinessId && !c.IsDeleted,
                cancellationToken);

        if (contact == null || !CampaignEligibility.HasValidEmail(contact))
        {
            await FinalizeRecipientAsync(
                recipient,
                campaign,
                CampaignQueueRecipientStatus.Skipped,
                utcNow,
                errorMessage: "Contacto sin correo válido.",
                resendMessageId: null,
                contact: null,
                subject: null,
                cancellationToken: cancellationToken);
            return true;
        }

        if (campaign.Status == "Queued")
        {
            if (_context.Database.IsRelational())
            {
                await _context.EmailCampaigns
                    .Where(c => c.Id == campaign.Id && c.Status == "Queued")
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(c => c.Status, "InProgress"),
                        cancellationToken);
            }
            else
            {
                campaign.Status = "InProgress";
                await _context.SaveChangesAsync(cancellationToken);
            }

            campaign.Status = "InProgress";
        }

        var subject = CampaignPersonalization.ApplySubjectTemplate(campaign.SubjectTemplate, recipient.ContactName);
        var html = CampaignEmailHtmlBuilder.Build(
            business.Name,
            business.LogoUrl,
            campaign.BodyText,
            campaign.ImageUrl,
            recipient.ContactName);

        try
        {
            var messageId = await _emailService.SendCampaignAsync(
                business,
                recipient.ContactEmail,
                subject,
                html,
                cancellationToken);

            await FinalizeRecipientAsync(
                recipient,
                campaign,
                CampaignQueueRecipientStatus.Sent,
                utcNow,
                errorMessage: null,
                resendMessageId: messageId,
                contact: contact,
                subject: subject,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            var message = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
            _logger.LogWarning(ex, "Campaign queue send failed. CampaignId={CampaignId}, ContactId={ContactId}", campaign.Id, contact.Id);

            await FinalizeRecipientAsync(
                recipient,
                campaign,
                CampaignQueueRecipientStatus.Failed,
                utcNow,
                errorMessage: message,
                resendMessageId: null,
                contact: contact,
                subject: subject,
                cancellationToken: cancellationToken);
        }

        _logger.LogInformation(
            "Campaign queue processed recipient {RecipientId}. Status={Status}, CampaignId={CampaignId}",
            recipient.Id,
            recipient.Status,
            campaign.Id);

        return true;
    }

    private async Task<Guid?> TryClaimNextRecipientIdAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var candidateId = await (
                from recipient in _context.EmailCampaignRecipients.AsNoTracking()
                join campaign in _context.EmailCampaigns.AsNoTracking() on recipient.CampaignId equals campaign.Id
                where recipient.Status == CampaignQueueRecipientStatus.Pending
                      && (campaign.Status == "Queued" || campaign.Status == "InProgress")
                orderby recipient.GlobalQueueOrder
                select recipient.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (candidateId == Guid.Empty)
            return null;

        if (_context.Database.IsRelational())
        {
            var claimed = await _context.EmailCampaignRecipients
                .Where(r => r.Id == candidateId && r.Status == CampaignQueueRecipientStatus.Pending)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(r => r.Status, CampaignQueueRecipientStatus.Processing)
                        .SetProperty(r => r.ProcessedAt, utcNow),
                    cancellationToken);

            return claimed == 1 ? candidateId : null;
        }

        var tracked = await _context.EmailCampaignRecipients
            .FirstOrDefaultAsync(
                r => r.Id == candidateId && r.Status == CampaignQueueRecipientStatus.Pending,
                cancellationToken);

        if (tracked == null)
            return null;

        tracked.Status = CampaignQueueRecipientStatus.Processing;
        tracked.ProcessedAt = utcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return tracked.Id;
    }

    private async Task RecoverStaleProcessingRecipientsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var staleBefore = utcNow - StaleProcessingThreshold;
        var staleRecipients = await _context.EmailCampaignRecipients
            .AsTracking()
            .Where(r => r.Status == CampaignQueueRecipientStatus.Processing
                        && r.ProcessedAt != null
                        && r.ProcessedAt < staleBefore)
            .ToListAsync(cancellationToken);

        if (staleRecipients.Count == 0)
            return;

        foreach (var recipient in staleRecipients)
        {
            recipient.Status = CampaignQueueRecipientStatus.Pending;
            recipient.ErrorMessage = "Reintento automático tras procesamiento interrumpido.";
        }

        await _context.SaveChangesAsync(cancellationToken);

        var campaignIds = staleRecipients.Select(r => r.CampaignId).Distinct().ToList();
        foreach (var campaignId in campaignIds)
            await CampaignQueueProgress.SyncCampaignFromRecipientsAsync(_context, campaignId, cancellationToken);

        _logger.LogWarning("Campaign queue recovered {Count} stale Processing recipients.", staleRecipients.Count);
    }

    private async Task FinalizeRecipientAsync(
        EmailCampaignRecipient recipient,
        EmailCampaign campaign,
        string finalStatus,
        DateTime utcNow,
        string? errorMessage,
        string? resendMessageId,
        ContactEntity? contact,
        string? subject,
        CancellationToken cancellationToken)
    {
        var finalized = await SetRecipientStatusAsync(
            recipient.Id,
            CampaignQueueRecipientStatus.Processing,
            finalStatus,
            utcNow,
            errorMessage,
            resendMessageId,
            cancellationToken);

        if (!finalized)
        {
            _logger.LogWarning(
                "Campaign queue finalize skipped for recipient {RecipientId}; status no longer Processing.",
                recipient.Id);
            return;
        }

        recipient.Status = finalStatus;
        recipient.ProcessedAt = utcNow;
        recipient.ErrorMessage = errorMessage;
        recipient.ResendMessageId = resendMessageId;

        if (contact != null && finalStatus == CampaignQueueRecipientStatus.Sent)
        {
            contact.LastMarketingEmailAt = utcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        await CampaignQueueProgress.SyncCampaignFromRecipientsAsync(_context, campaign.Id, cancellationToken);

        if (contact != null && !string.IsNullOrWhiteSpace(subject))
            await TryPersistCampaignLogAsync(campaign, contact, subject, finalStatus, utcNow, resendMessageId, errorMessage, cancellationToken);
    }

    private async Task MarkRecipientAsync(
        Guid recipientId,
        string finalStatus,
        DateTime utcNow,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        await SetRecipientStatusAsync(
            recipientId,
            expectedCurrentStatus: null,
            finalStatus,
            utcNow,
            errorMessage,
            resendMessageId: null,
            cancellationToken);
    }

    private async Task<bool> SetRecipientStatusAsync(
        Guid recipientId,
        string? expectedCurrentStatus,
        string finalStatus,
        DateTime utcNow,
        string? errorMessage,
        string? resendMessageId,
        CancellationToken cancellationToken)
    {
        if (_context.Database.IsRelational())
        {
            var query = _context.EmailCampaignRecipients.Where(r => r.Id == recipientId);
            if (!string.IsNullOrEmpty(expectedCurrentStatus))
                query = query.Where(r => r.Status == expectedCurrentStatus);

            var rows = await query.ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(r => r.Status, finalStatus)
                    .SetProperty(r => r.ProcessedAt, utcNow)
                    .SetProperty(r => r.ErrorMessage, errorMessage)
                    .SetProperty(r => r.ResendMessageId, resendMessageId),
                cancellationToken);

            return rows == 1;
        }

        var tracked = await _context.EmailCampaignRecipients
            .FirstOrDefaultAsync(r => r.Id == recipientId, cancellationToken);

        if (tracked == null)
            return false;

        if (!string.IsNullOrEmpty(expectedCurrentStatus) && tracked.Status != expectedCurrentStatus)
            return false;

        tracked.Status = finalStatus;
        tracked.ProcessedAt = utcNow;
        tracked.ErrorMessage = errorMessage;
        tracked.ResendMessageId = resendMessageId;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task TryPersistCampaignLogAsync(
        EmailCampaign campaign,
        ContactEntity contact,
        string subject,
        string finalStatus,
        DateTime utcNow,
        string? resendMessageId,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        var log = new ContactEmailCampaignLog
        {
            Id = Guid.NewGuid(),
            BusinessId = campaign.BusinessId,
            ContactId = contact.Id,
            SentAt = utcNow,
            Subject = subject,
            InactiveDaysUsed = campaign.InactiveDaysUsed,
            QuietDaysUsed = campaign.QuietDaysUsed,
            Status = finalStatus == CampaignQueueRecipientStatus.Sent ? "Sent" : "Failed",
            ResendMessageId = resendMessageId,
            ErrorMessage = errorMessage
        };

        try
        {
            _context.ContactEmailCampaignLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Campaign log persist failed after recipient {RecipientId} was finalized as {Status}. " +
                "Verify ContactEmailCampaignLogs exists in Supabase.",
                contact.Id,
                finalStatus);
        }
    }
}
