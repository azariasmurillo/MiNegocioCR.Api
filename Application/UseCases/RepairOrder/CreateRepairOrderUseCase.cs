using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
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

        if (!request.ContactId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
                throw new ArgumentException("Se requiere el nombre del cliente o un ContactId.", nameof(request.CustomerName));
            if (string.IsNullOrWhiteSpace(PhoneSanitizer.Sanitize(request.CustomerPhone)))
                throw new ArgumentException("Se requiere el teléfono del cliente o un ContactId.", nameof(request.CustomerPhone));
        }

        var contact = await RepairOrderContactHelper.ResolveContactForCreateAsync(
            _context,
            businessId,
            request.ContactId,
            request.CustomerName,
            request.CustomerPhone,
            request.CustomerEmail);

        var order = new RepairOrder
        {
            BusinessId = businessId,
            OrderNumber = orderNumber,
            ContactId = contact.Id,
            Contact = contact,
            DeviceDescription = request.DeviceDescription,
            ProblemDescription = request.ProblemDescription,
            Status = (int)RepairOrderStatus.Pending
        };

        _context.RepairOrders.Add(order);

        await _context.SaveChangesAsync(CancellationToken.None);
        await transaction.CommitAsync();
        var business = await _context.Businesses.FindAsync(businessId);
        if(business == null) 
            throw new NotFoundException("Business", "Business not found.");
        
        await _notificationService.OrderCreatedAsync(business, order);

        return new
        {
            order.Id,
            order.OrderNumber,
            Status = "Pending",
            order.ContactId,
            Contact = new
            {
                order.Contact.Id,
                Name = order.Contact.Name,
                Phone = order.Contact.Phone,
                Email = order.Contact.Email
            }
        };
    }
}
