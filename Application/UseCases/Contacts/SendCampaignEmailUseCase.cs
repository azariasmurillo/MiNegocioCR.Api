using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class SendCampaignEmailUseCase : ISendCampaignEmailUseCase
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public SendCampaignEmailUseCase(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<SendCampaignEmailResultDto> Execute(Guid businessId, SendCampaignEmailRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.ContactId == Guid.Empty)
            throw new ArgumentException("ContactId is required.", nameof(request.ContactId));
        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new ArgumentException("Subject is required.", nameof(request.Subject));

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

        var quota = await GetCampaignPreviewUseCase.BuildQuotaAsync(_context, utcNow);
        if (quota.RemainingToday <= 0)
            throw new InvalidOperationException(
                $"Límite diario alcanzado ({quota.DailyLimit} correos). Probá mañana o reducí la audiencia.");

        var contact = await _context.Contacts
            .AsTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ContactId && c.BusinessId == businessId && !c.IsDeleted)
            ?? throw new ArgumentException("Contact not found.");

        if (!CampaignEligibility.IsEligible(contact, inactiveDays, quietDays, utcNow, audienceMode))
            throw new InvalidOperationException(
                audienceMode == CampaignAudienceMode.AllWithEmail
                    ? "El contacto ya no es elegible (sin correo válido o eliminado)."
                    : "El contacto ya no es elegible para esta campaña (activo, sin email o en período de quietud).");

        var html = CampaignEmailHtmlBuilder.Build(
            business.Name,
            business.LogoUrl,
            request.BodyText,
            request.ImageUrl);

        var log = new ContactEmailCampaignLog
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ContactId = contact.Id,
            SentAt = utcNow,
            Subject = request.Subject.Trim(),
            InactiveDaysUsed = inactiveDays,
            QuietDaysUsed = quietDays
        };

        try
        {
            var messageId = await _emailService.SendCampaignAsync(
                business,
                contact.Email!.Trim(),
                log.Subject,
                html);

            log.Status = "Sent";
            log.ResendMessageId = messageId;
            contact.LastMarketingEmailAt = utcNow;
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.ErrorMessage = ex.Message.Length > 500 ? ex.Message[..500] : ex.Message;
            _context.ContactEmailCampaignLogs.Add(log);
            await _context.SaveChangesAsync(CancellationToken.None);

            var failedQuota = await GetCampaignPreviewUseCase.BuildQuotaAsync(_context, utcNow);
            return new SendCampaignEmailResultDto
            {
                ContactId = contact.Id,
                Status = "Failed",
                ErrorMessage = log.ErrorMessage,
                Quota = failedQuota
            };
        }

        _context.ContactEmailCampaignLogs.Add(log);
        await _context.SaveChangesAsync(CancellationToken.None);

        var updatedQuota = await GetCampaignPreviewUseCase.BuildQuotaAsync(_context, utcNow);
        return new SendCampaignEmailResultDto
        {
            ContactId = contact.Id,
            Status = "Sent",
            ResendMessageId = log.ResendMessageId,
            Quota = updatedQuota
        };
    }
}
