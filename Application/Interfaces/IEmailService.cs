using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Api.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmail(string toEmail, string resetLink);
    Task SendTestEmail(string toEmail);

    Task SendAsync(
        BusinessEntity business,
        string toEmail,
        string subject,
        string body);

    /// <summary>Envío de campaña con remitente del negocio y Reply-To opcional. Retorna el id del mensaje en Resend.</summary>
    Task<string?> SendCampaignAsync(
        BusinessEntity business,
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
