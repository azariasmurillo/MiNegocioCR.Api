using MiNegocioCR.Api.Domain.Entities;
using BusinessEntity = MiNegocioCR.Api.Domain.Entities.Business;

namespace MiNegocioCR.Api.Application.Interfaces;
public interface INotificationService
{
    Task OrderCreatedAsync(BusinessEntity business, RepairOrder order);
    Task OrderProcessedAsync(BusinessEntity business, RepairOrder order);
    Task OrderDeliveredAsync(BusinessEntity business, RepairOrder order);
    Task OrderCancelledAsync(BusinessEntity business, RepairOrder order);
}