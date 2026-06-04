using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.InternetOrders;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.InternetOrders;

public class SendInternetOrderEmailUseCase : ISendInternetOrderEmailUseCase
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public SendInternetOrderEmailUseCase(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task Execute(Guid businessId, Guid orderId, string htmlContent, string? destinationEmail = null)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
            throw new ArgumentException("El contenido HTML es obligatorio.", nameof(htmlContent));

        var order = await _context.InternetOrders
            .Include(o => o.Contact)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.BusinessId == businessId);

        if (order == null)
            throw new NotFoundException("InternetOrder", "Pedido no encontrado.");

        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId);
        if (business == null)
            throw new NotFoundException("Business", "Negocio no encontrado.");

        if (!business.EnableEmailNotifications)
            throw new InvalidOperationException("Las notificaciones por correo están desactivadas para este negocio.");

        var toEmail = string.IsNullOrWhiteSpace(destinationEmail)
            ? order.Contact.Email
            : destinationEmail.Trim();

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new InvalidOperationException("El contacto no tiene correo electrónico.");

        var subject = $"Pedido internet #{order.OrderNumber}";
        await _emailService.SendAsync(business, toEmail, subject, htmlContent);
    }
}
