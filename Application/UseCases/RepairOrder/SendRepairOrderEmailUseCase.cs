using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

public class SendRepairOrderEmailUseCase : ISendRepairOrderEmailUseCase
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public SendRepairOrderEmailUseCase(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task Execute(Guid id, string htmlContent, string? destinationEmail = null)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("htmlContent is required.", nameof(htmlContent));

        var order = await _context.RepairOrders
            .Include(r => r.Contact)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (order == null)
            throw new NotFoundException("RepairOrder", "Repair order not found.");

        var business = await _context.Businesses
            .FirstOrDefaultAsync(b => b.Id == order.BusinessId);

        if (business == null)
            throw new NotFoundException("Business", "Business not found.");

        if (!business.EnableEmailNotifications)
            throw new InvalidOperationException("Email notifications are disabled for this business.");

        var toEmail = string.IsNullOrWhiteSpace(destinationEmail)
            ? order.Contact.Email
            : destinationEmail.Trim();

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new InvalidOperationException("Contact does not have an email address.");

        var subject = $"Orden #{order.OrderNumber}";

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
