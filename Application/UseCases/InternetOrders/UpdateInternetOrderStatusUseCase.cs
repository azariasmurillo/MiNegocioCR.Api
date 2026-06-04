using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.InternetOrders;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.InternetOrders;

public class UpdateInternetOrderStatusUseCase : IUpdateInternetOrderStatusUseCase
{
    private readonly IAppDbContext _context;
    private readonly IInternetOrderNotificationService _notifications;

    public UpdateInternetOrderStatusUseCase(
        IAppDbContext context,
        IInternetOrderNotificationService notifications)
    {
        _context = context;
        _notifications = notifications;
    }

    public async Task<object> Execute(Guid businessId, Guid id, UpdateInternetOrderStatusRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var order = await _context.InternetOrders
            .AsTracking()
            .Include(o => o.Contact)
            .Include(o => o.Lines)
            .Include(o => o.Advances)
            .FirstOrDefaultAsync(o => o.BusinessId == businessId && o.Id == id);

        if (order == null)
            throw new NotFoundException("InternetOrder", "Pedido no encontrado.");

        var current = (InternetOrderStatus)order.Status;
        var next = request.NewStatus;

        if (current == next)
            return new { order.Id, order.OrderNumber, Status = next.ToString() };

        if (!InternetOrderStatusRules.IsValidTransition(current, next))
            throw new InvalidStatusTransitionException(current.ToString(), next.ToString());

        var utcNow = DateTime.UtcNow;
        order.Status = (int)next;
        order.UpdatedAt = utcNow;

        if (next == InternetOrderStatus.Purchased)
            order.PurchasedAt = utcNow;
        if (next == InternetOrderStatus.Received)
            order.ReceivedAt = utcNow;
        if (next == InternetOrderStatus.Delivered)
            order.DeliveredAt = utcNow;
        if (next == InternetOrderStatus.Cancelled)
        {
            order.CancelledAt = utcNow;
            if (!string.IsNullOrWhiteSpace(request.RefundNote))
                order.RefundNote = request.RefundNote.Trim();
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null)
            throw new NotFoundException("Business", "Negocio no encontrado.");

        if (next == InternetOrderStatus.Purchased)
            await _notifications.NotifyPurchasedAsync(business, order);
        if (next == InternetOrderStatus.Received)
            await _notifications.NotifyReceivedAsync(business, order);

        return new { order.Id, order.OrderNumber, Status = next.ToString() };
    }
}
