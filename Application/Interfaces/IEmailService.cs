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
}
