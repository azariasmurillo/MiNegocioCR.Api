using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Exceptions;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Sales;

namespace MiNegocioCR.Api.Application.UseCases.Sales;

public class SendSaleEmailUseCase : ISendSaleEmailUseCase
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public SendSaleEmailUseCase(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task Execute(Guid id, string htmlContent, string? destinationEmail = null)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("htmlContent is required.", nameof(htmlContent));

        var sale = await _context.Sales
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.BusinessId,
                s.InvoiceNumber,
                ContactEmail = s.Contact != null ? s.Contact.Email : null
            })
            .FirstOrDefaultAsync();

        if (sale is null)
            throw new NotFoundException("Sale", "Sale not found.");

        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == sale.BusinessId);

        if (business == null)
            throw new NotFoundException("Business", "Business not found.");

        if (!business.EnableEmailNotifications)
            throw new InvalidOperationException("Email notifications are disabled for this business.");

        var toEmail = string.IsNullOrWhiteSpace(destinationEmail)
            ? sale.ContactEmail
            : destinationEmail.Trim();

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new InvalidOperationException("Sale contact does not have an email address.");

        var subject = $"Factura #{sale.InvoiceNumber}";

        try
        {
            await _emailService.SendAsync(business, toEmail, subject, htmlContent);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"SMTP send failed: {ex.Message}");
        }
    }
}
