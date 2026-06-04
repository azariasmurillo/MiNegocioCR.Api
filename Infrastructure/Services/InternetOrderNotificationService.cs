using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class InternetOrderNotificationService : IInternetOrderNotificationService
{
    private readonly IEmailService _emailService;

    public InternetOrderNotificationService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public Task NotifyPurchasedAsync(BusinessEntity business, InternetOrder order) =>
        SendAsync(
            business,
            order,
            "Pedido comprado",
            "Tu pedido fue comprado. Te avisaremos cuando llegue al país. Abajo está el detalle de tu pedido.");

    public Task NotifyReceivedAsync(BusinessEntity business, InternetOrder order) =>
        SendAsync(
            business,
            order,
            "Pedido recibido",
            "Tu pedido ya llegó. Contactanos para coordinar la entrega. Revisá el detalle y el saldo pendiente.");

    private async Task SendAsync(BusinessEntity business, InternetOrder order, string title, string introMessage)
    {
        var email = order.Contact?.Email;
        if (!business.EnableEmailNotifications || string.IsNullOrWhiteSpace(email))
            return;

        var subject = $"{title} — #{order.OrderNumber}";
        var body = InternetOrderEmailHtmlBuilder.Build(business, order, title, introMessage);

        await _emailService.SendAsync(business, email, subject, body);
    }
}
