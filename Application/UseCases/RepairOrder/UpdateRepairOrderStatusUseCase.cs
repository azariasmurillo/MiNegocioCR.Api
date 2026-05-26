using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Entities;
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

    public async Task<object> Execute(Guid businessId, Guid id, UpdateStatusRequestDto request)
    {
        var order = await _context.RepairOrders
            .AsTracking()
            .Include(o => o.Contact)
            .FirstOrDefaultAsync(o => o.BusinessId == businessId && o.Id == id);

        if (order == null)
        {
            throw new NotFoundException("RepairOrder", "Order not found");
        }

        var currentStatus = (RepairOrderStatus)order.Status;
        var newStatus = request.NewStatus;

        if (currentStatus == newStatus)
        {
            return new
            {
                order.Id,
                order.OrderNumber,
                Status = newStatus.ToString()
            };
        }

        if (!RepairOrderStatusRules.IsValidTransition(currentStatus, newStatus))
        {
            throw new InvalidStatusTransitionException(currentStatus.ToString(), newStatus.ToString());
        }

        order.Status = (int)newStatus;
        order.UpdatedAt = DateTime.UtcNow;
        if (newStatus == RepairOrderStatus.Cancelled)
            order.IsActive = false;

        await _context.SaveChangesAsync(CancellationToken.None);
        var business = await _context.Businesses.FindAsync(order.BusinessId);
        if (business == null)
            throw new NotFoundException("Business", "Business not found");

        if (newStatus == RepairOrderStatus.Processed)        
            await _notificationService.OrderProcessedAsync(business, order);                    

        if (newStatus == RepairOrderStatus.Delivered)
            await _notificationService.OrderDeliveredAsync(business,order);

        if (newStatus == RepairOrderStatus.Cancelled)
            await _notificationService.OrderCancelledAsync(business,order);

        return new
        {
            order.Id,
            order.OrderNumber,
            Status = newStatus.ToString()
        };
    }
}