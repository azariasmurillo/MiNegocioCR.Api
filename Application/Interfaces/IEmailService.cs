using MiNegocioCR.Api.Domain.Entities;

public interface IEmailService
{
    Task SendAsync(
        Business business,
        string toEmail,
        string subject,
        string body);
}