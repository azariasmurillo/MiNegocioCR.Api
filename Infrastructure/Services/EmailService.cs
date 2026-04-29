using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    public async Task SendAsync(
        Business business,
        string toEmail,
        string subject,
        string body)
    {
        if (!business.EnableEmailNotifications)
            throw new InvalidOperationException("Email notifications are disabled for this business.");

        if (string.IsNullOrWhiteSpace(business.SmtpHost) ||
            !business.SmtpPort.HasValue ||
            string.IsNullOrWhiteSpace(business.SmtpUsername) ||
            string.IsNullOrWhiteSpace(business.SmtpPassword) ||
            string.IsNullOrWhiteSpace(business.SmtpFromEmail))
        {
            throw new InvalidOperationException("SMTP configuration is incomplete for this business.");
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(
            business.SmtpFromName ?? business.Name,
            business.SmtpFromEmail));

        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        email.Body = new TextPart("html")
        {
            Text = body
        };

        using var smtp = new SmtpClient();
        var socketOptions = business.EnableSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.None;

        await smtp.ConnectAsync(
            business.SmtpHost,
            business.SmtpPort.Value,
            socketOptions);

        await smtp.AuthenticateAsync(
            business.SmtpUsername,
            business.SmtpPassword);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}