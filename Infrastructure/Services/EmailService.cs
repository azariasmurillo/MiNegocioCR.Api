using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class EmailService : IEmailService
{
    public async Task SendAsync(
        Business business,
        string toEmail,
        string subject,
        string body)
    {
        if (string.IsNullOrEmpty(business.SmtpHost))
            return; // No SMTP configurado

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(
            business.SmtpFromName,
            business.SmtpFromEmail));

        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        email.Body = new TextPart("html")
        {
            Text = body
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            business.SmtpHost,
            business.SmtpPort!.Value,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            business.SmtpUsername,
            business.SmtpPassword);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}