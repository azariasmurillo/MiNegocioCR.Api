using MiNegocioCR.Api.Aplication.DTOs;
using MiNegocioCR.Api.Aplication.Interfaces;
using MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders;
using MiNegocioCR.Api.Aplication.Services;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;


public class UpdateRepairOrderStatusUseCase : IUpdateRepairOrderStatusUseCase
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;

    public UpdateRepairOrderStatusUseCase(
        IAppDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<object> Execute(Guid id, UpdateStatusRequestDto request)
    {
        var order = await _context.RepairOrders.FindAsync(id);

        if (order == null)
        {
            throw new NotFoundException("RepairOrder", "Order not found");
        }

        var currentStatus = (RepairOrderStatus)order.Status;
        var newStatus = request.NewStatus;

        if (!IsValidTransition(currentStatus, newStatus))
        {
            throw new InvalidStatusTransitionException(currentStatus.ToString(), newStatus.ToString());
        }

        order.Status = (int)newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(CancellationToken.None);

        if (newStatus == RepairOrderStatus.Processed)
            await _notificationService.SendOrderProcessedAsync(order);

        if (newStatus == RepairOrderStatus.Delivered)
            await _notificationService.SendOrderDeliveredAsync(order);

        if (newStatus == RepairOrderStatus.Cancelled)
            await _notificationService.SendOrderCancelledAsync(order);

        return new
        {
            order.Id,
            order.OrderNumber,
            Status = newStatus.ToString()
        };
    }

    private bool IsValidTransition(RepairOrderStatus current, RepairOrderStatus next)
    {
        return (current, next) switch
        {
            (RepairOrderStatus.Pending, RepairOrderStatus.InProcess) => true,
            (RepairOrderStatus.Pending, RepairOrderStatus.Cancelled) => true,
            (RepairOrderStatus.InProcess, RepairOrderStatus.Processed) => true,
            (RepairOrderStatus.InProcess, RepairOrderStatus.Cancelled) => true,
            (RepairOrderStatus.Processed, RepairOrderStatus.Delivered) => true,
            (RepairOrderStatus.Processed, RepairOrderStatus.Cancelled) => true,
            _ => false
        };
    }
}