using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Aplication.Services;

public interface INotificationService
{
    Task SendOrderCreatedAsync(RepairOrder order);
    Task SendOrderProcessedAsync(RepairOrder order);
    Task SendOrderDeliveredAsync(RepairOrder order);
    Task SendOrderCancelledAsync(RepairOrder order);
}