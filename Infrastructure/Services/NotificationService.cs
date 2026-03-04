using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    

    public NotificationService(
    IEmailService emailService)
    {
        _emailService = emailService;        
    }

    public async Task SendOrderCreatedAsync(RepairOrder order)
    {
        if (order is null)
            return;

        if (order.Business is null)
            return; // Business no cargado o no configurado — no enviar notificaciones

        var subject = $"Orden #{order.OrderNumber} creada";
        var body = $"Su orden fue creada correctamente.";

        if (order.Business.EnableEmailNotifications && !string.IsNullOrWhiteSpace(order.CustomerEmail))
        {
            await _emailService.SendAsync(
                order.Business,
                order.CustomerEmail,
                subject,
                body);
        }        
    }

    public Task SendOrderProcessedAsync(RepairOrder order)
    {
        Console.WriteLine($"[EMAIL/WHATSAPP] Order Processed #{order.OrderNumber}");
        return Task.CompletedTask;
    }

    public Task SendOrderDeliveredAsync(RepairOrder order)
    {
        Console.WriteLine($"[EMAIL/WHATSAPP] Order Delivered #{order.OrderNumber}");
        return Task.CompletedTask;
    }

    public Task SendOrderCancelledAsync(RepairOrder order)
    {
        Console.WriteLine($"[EMAIL/WHATSAPP] Order Cancelled #{order.OrderNumber}");
        return Task.CompletedTask;
    }   
}