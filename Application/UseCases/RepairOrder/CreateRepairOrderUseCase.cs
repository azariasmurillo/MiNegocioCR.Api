using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

public class CreateRepairOrderUseCase : ICreateRepairOrderUseCase
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;

    public CreateRepairOrderUseCase(
        IAppDbContext context,
        INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<object> Execute(Guid businessId, CreateRepairOrderRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        using var transaction = await (_context as DbContext)!
            .Database.BeginTransactionAsync();

        var settings = await _context.BusinessSettings
            .FromSqlRaw(
                "SELECT * FROM \"BusinessSettings\" WHERE \"BusinessId\" = {0} FOR UPDATE",
                businessId)
            .FirstOrDefaultAsync();

        if (settings == null)
            throw new NotFoundException("BusinessSettings", "Business settings not found.");

        var orderNumber = settings.NextOrderNumber;
        settings.NextOrderNumber++;

        var order = new RepairOrder
        {
            BusinessId = businessId,
            OrderNumber = orderNumber,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            CustomerEmail = request.CustomerEmail,
            DeviceDescription = request.DeviceDescription,
            ProblemDescription = request.ProblemDescription,
            Status = (int)RepairOrderStatus.Pending
        };

        _context.RepairOrders.Add(order);

        await _context.SaveChangesAsync(CancellationToken.None);
        await transaction.CommitAsync();

        await _notificationService.SendOrderCreatedAsync(order);

        return new
        {
            order.Id,
            order.OrderNumber,
            Status = "Pending"
        };
    }
}
