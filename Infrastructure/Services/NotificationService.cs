using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IEmailService _emailService;

        public NotificationService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task OrderCreatedAsync(Business business, RepairOrder order)
        {
            var email = order.Contact?.Email;
            if (!business.EnableEmailNotifications || string.IsNullOrEmpty(email))
                return;

            var subject = $"Orden #{order.Id} creada";

            var body = $@"
                <h2>Orden creada</h2>
                <p>Su orden fue creada correctamente.</p>
                <p><b>Número:</b> {order.Id}</p>
            ";

            await _emailService.SendAsync(business, email, subject, body);
        }

        public async Task OrderProcessedAsync(Business business, RepairOrder order)
        {
            var email = order.Contact?.Email;
            if (!business.EnableEmailNotifications || string.IsNullOrEmpty(email))
                return;

            var subject = $"Orden #{order.Id} en proceso";

            var body = $@"
                <h2>Orden en proceso</h2>
                <p>Su equipo está siendo procesado.</p>
                <p><b>Número:</b> {order.Id}</p>
            ";

            await _emailService.SendAsync(business, email, subject, body);
        }

        public async Task OrderDeliveredAsync(Business business, RepairOrder order)
        {
            var email = order.Contact?.Email;
            if (!business.EnableEmailNotifications || string.IsNullOrEmpty(email))
                return;

            var subject = $"Orden #{order.Id} lista para entrega";

            var body = $@"
                <h2>Orden lista</h2>
                <p>Su equipo está listo para ser retirado.</p>
                <p><b>Número:</b> {order.Id}</p>
            ";

            await _emailService.SendAsync(business, email, subject, body);
        }

        public async Task OrderCancelledAsync(Business business, RepairOrder order)
        {
            var email = order.Contact?.Email;
            if (!business.EnableEmailNotifications || string.IsNullOrEmpty(email))
                return;

            var subject = $"Orden #{order.Id} cancelada";

            var body = $@"
                <h2>Orden cancelada</h2>
                <p>Su orden fue cancelada.</p>
                <p><b>Número:</b> {order.Id}</p>
            ";

            await _emailService.SendAsync(business, email, subject, body);
        }
    }
}