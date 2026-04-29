using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using System.Text;

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

            var subject = $"Orden #{order.OrderNumber} creada";
            var body = BuildStatusEmailHtml(
                business,
                order,
                "Orden creada",
                "Su orden fue creada correctamente. Le notificaremos cada avance por este medio.");
            await _emailService.SendAsync(business, email, subject, body);
        }

        public async Task OrderProcessedAsync(Business business, RepairOrder order)
        {
            var email = order.Contact?.Email;
            if (!business.EnableEmailNotifications || string.IsNullOrEmpty(email))
                return;

            var subject = $"Orden #{order.OrderNumber} en proceso";
            var body = BuildStatusEmailHtml(
                business,
                order,
                "Orden en proceso",
                "Su equipo está siendo procesado por nuestro equipo técnico.");
            await _emailService.SendAsync(business, email, subject, body);
        }

        public async Task OrderDeliveredAsync(Business business, RepairOrder order)
        {
            var email = order.Contact?.Email;
            if (!business.EnableEmailNotifications || string.IsNullOrEmpty(email))
                return;

            var subject = $"Orden #{order.OrderNumber} lista para entrega";
            var body = BuildStatusEmailHtml(
                business,
                order,
                "Orden lista para entrega",
                "Su equipo está listo para ser retirado. Gracias por confiar en nosotros.");
            await _emailService.SendAsync(business, email, subject, body);
        }

        public async Task OrderCancelledAsync(Business business, RepairOrder order)
        {
            var email = order.Contact?.Email;
            if (!business.EnableEmailNotifications || string.IsNullOrEmpty(email))
                return;

            var subject = $"Orden #{order.OrderNumber} cancelada";
            var body = BuildStatusEmailHtml(
                business,
                order,
                "Orden cancelada",
                "La orden fue cancelada. Si tiene consultas, responda este correo o contáctenos.");
            await _emailService.SendAsync(business, email, subject, body);
        }

        private static string BuildStatusEmailHtml(
            Business business,
            RepairOrder order,
            string title,
            string message)
        {
            var statusLabel = ((RepairOrderStatus)order.Status).ToString();
            var customerName = order.Contact?.Name ?? "Cliente";
            var customerPhone = order.Contact?.Phone ?? "-";
            var businessPhone = string.IsNullOrWhiteSpace(business.Phone) ? "-" : business.Phone;
            var businessEmail = string.IsNullOrWhiteSpace(business.PublicEmail) ? "-" : business.PublicEmail;
            var problem = string.IsNullOrWhiteSpace(order.ProblemDescription) ? "No especificado" : order.ProblemDescription;
            var device = string.Join(" ", new[] { order.DeviceType, order.Brand, order.Model }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            if (string.IsNullOrWhiteSpace(device))
                device = "No especificado";

            var sb = new StringBuilder();
            sb.Append("<html><body style=\"margin:0;padding:0;background:#f5f7fb;font-family:Arial,Helvetica,sans-serif;color:#1f2937;\">");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f5f7fb;padding:24px 0;\">");
            sb.Append("<tr><td align=\"center\">");
            sb.Append("<table role=\"presentation\" width=\"700\" cellspacing=\"0\" cellpadding=\"0\" style=\"width:700px;max-width:700px;background:#ffffff;border:1px solid #e5e7eb;border-radius:8px;overflow:hidden;\">");
            sb.Append("<tr><td style=\"padding:20px 24px;background:#111827;color:#ffffff;\">");
            if (!string.IsNullOrWhiteSpace(business.LogoUrl))
            {
                sb.Append("<div style=\"margin-bottom:10px;\">");
                sb.Append($"<img src=\"{business.LogoUrl}\" alt=\"Logo negocio\" style=\"max-height:56px;display:block;\"/>");
                sb.Append("</div>");
            }
            sb.Append($"<div style=\"font-size:20px;font-weight:700;\">{business.Name}</div>");
            sb.Append($"<div style=\"font-size:13px;opacity:0.9;margin-top:6px;\">{title}</div>");
            sb.Append("</td></tr>");
            sb.Append("<tr><td style=\"padding:20px 24px;\">");
            sb.Append($"<p style=\"margin:0 0 12px;font-size:15px;\">Hola {customerName},</p>");
            sb.Append($"<p style=\"margin:0 0 18px;font-size:14px;line-height:1.5;\">{message}</p>");
            sb.Append("<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border-collapse:collapse;\">");
            sb.Append(Row("Orden", order.OrderNumber));
            sb.Append(Row("Estado", statusLabel));
            sb.Append(Row("Cliente", customerName));
            sb.Append(Row("Telefono", customerPhone));
            sb.Append(Row("Equipo", device));
            sb.Append(Row("Problema reportado", problem));
            sb.Append("</table>");
            sb.Append("</td></tr>");
            sb.Append("<tr><td style=\"padding:16px 24px;background:#f9fafb;border-top:1px solid #e5e7eb;\">");
            sb.Append($"<div style=\"font-size:13px;color:#374151;\"><strong>Contacto:</strong> {businessPhone} · {businessEmail}</div>");
            sb.Append("<div style=\"font-size:12px;color:#6b7280;margin-top:6px;\">Este correo fue generado automaticamente por MiNegocioCR.</div>");
            sb.Append("</td></tr>");
            sb.Append("</table>");
            sb.Append("</td></tr></table></body></html>");
            return sb.ToString();
        }

        private static string Row(string label, string value)
        {
            return $@"
                <tr>
                    <td style=""padding:8px 10px;border:1px solid #e5e7eb;background:#f9fafb;font-size:13px;width:180px;""><strong>{label}</strong></td>
                    <td style=""padding:8px 10px;border:1px solid #e5e7eb;font-size:13px;"">{value}</td>
                </tr>";
        }
    }
}