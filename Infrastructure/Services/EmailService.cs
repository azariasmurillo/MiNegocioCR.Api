using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiNegocioCR.Api.Application.Configuration;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;
using Resend;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly ResendSettings _settings;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IResend resend,
        IOptions<ResendSettings> settings,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendPasswordResetEmail(string toEmail, string resetLink)
    {
        var safeLink = System.Net.WebUtility.HtmlEncode(resetLink);
        var body =
            "<h2>Recuperación de contraseña</h2>" +
            "<p>Recibimos una solicitud para restablecer tu contraseña.</p>" +
            $"<p><a href=\"{safeLink}\" style=\"display:inline-block;padding:10px 16px;background:#2b6ef2;color:#ffffff;text-decoration:none;border-radius:6px;\">Restablecer contraseña</a></p>" +
            "<p>Si el botón no funciona, copiá y pegá este enlace en tu navegador:</p>" +
            $"<p>{safeLink}</p>" +
            "<p>Este enlace expira en 10 minutos.</p>";

        await SendWithResendAsync(toEmail, "Recuperación de contraseña", body);
    }

    public async Task SendTestEmail(string toEmail)
    {
        const string subject = "Test Resend - MiNegocioCR";
        const string body = "<h2>Prueba Resend</h2><p>Este correo confirma que la configuración de Resend está funcionando.</p>";
        await SendWithResendAsync(toEmail, subject, body);
    }

    public async Task SendAsync(
        Business business,
        string toEmail,
        string subject,
        string body)
    {
        if (!business.EnableEmailNotifications)
        {
            throw new InvalidOperationException("Email notifications are disabled for this business.");
        }

        await SendWithResendAsync(toEmail, subject, body);
    }

    public async Task<string?> SendCampaignAsync(
        Business business,
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (!business.EnableEmailNotifications)
            throw new InvalidOperationException("Email notifications are disabled for this business.");

        if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("Resend config missing required values: ApiKey and FromEmail.");
        }

        var displayName = string.IsNullOrWhiteSpace(business.Name) ? _settings.FromName : business.Name.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = "MiNegocioCR";

        var email = new EmailMessage
        {
            From = $"{displayName} <{_settings.FromEmail}>",
            To = new[] { toEmail },
            Subject = subject,
            HtmlBody = htmlBody
        };

        if (!string.IsNullOrWhiteSpace(business.PublicEmail) && business.PublicEmail.Contains('@'))
            email.ReplyTo = business.PublicEmail.Trim();

        _logger.LogInformation(
            "Resend campaign send. from={From}, replyTo={ReplyTo}, to={To}, subject={Subject}",
            email.From,
            email.ReplyTo,
            toEmail,
            subject);

        var response = await _resend.EmailSendAsync(email, cancellationToken);
        if (!response.Success)
        {
            throw new InvalidOperationException(
                response.Exception?.Message ?? "Resend failed to send campaign email.");
        }

        _logger.LogInformation("Resend campaign email sent. id={MessageId}", response.Content);
        return response.Content.ToString();
    }

    private async Task SendWithResendAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey) ||
            string.IsNullOrWhiteSpace(_settings.FromEmail))
        {
            throw new InvalidOperationException("Resend config missing required values: ApiKey and FromEmail.");
        }

        var fromName = string.IsNullOrWhiteSpace(_settings.FromName) ? "MiNegocioCR" : _settings.FromName;
        var email = new EmailMessage
        {
            From = $"{fromName} <{_settings.FromEmail}>",
            To = new[] { toEmail },
            Subject = subject,
            HtmlBody = htmlBody
        };

        _logger.LogInformation(
            "Resend send phase. from={From}, to={To}, subject={Subject}",
            _settings.FromEmail,
            toEmail,
            subject);
        var response = await _resend.EmailSendAsync(email);
        if (!response.Success)
        {
            throw new InvalidOperationException(
                response.Exception?.Message ?? "Resend failed to send email.");
        }
        _logger.LogInformation("Resend email sent. id={MessageId}", response.Content);
    }
}
