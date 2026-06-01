using Microsoft.Extensions.Logging;
using MiNegocioCR.Api.Application.Interfaces;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Api.Infrastructure.Services;

/// <summary>
/// Servicio de email para desarrollo local. En vez de enviar correos reales,
/// imprime el contenido en los logs de la consola.
/// </summary>
public class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmail(string toEmail, string resetLink)
    {
        _logger.LogWarning(
            "[DEV EMAIL] Recuperación de contraseña para {Email}\n" +
            "  Link: {ResetLink}",
            toEmail, resetLink);
        return Task.CompletedTask;
    }

    public Task SendTestEmail(string toEmail)
    {
        _logger.LogWarning("[DEV EMAIL] Email de prueba enviado a {Email}", toEmail);
        return Task.CompletedTask;
    }

    public Task SendAsync(BusinessEntity business, string toEmail, string subject, string body)
    {
        _logger.LogWarning(
            "[DEV EMAIL] Para: {Email} | Asunto: {Subject}\n{Body}",
            toEmail, subject, body);
        return Task.CompletedTask;
    }

    public Task<string?> SendCampaignAsync(
        BusinessEntity business,
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "[DEV EMAIL CAMPAIGN] Negocio: {Business} | Reply-To: {ReplyTo} | Para: {Email} | Asunto: {Subject}\n{Body}",
            business.Name,
            business.PublicEmail,
            toEmail,
            subject,
            htmlBody);
        return Task.FromResult<string?>("dev-campaign-message-id");
    }
}
