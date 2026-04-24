using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using RepairOrderEntity = MiNegocioCR.Api.Domain.Entities.RepairOrder;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

public class CreateRepairOrderUseCase : ICreateRepairOrderUseCase
{
    private readonly IAppDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IGetRepairOrderByIdUseCase _getRepairOrderById;

    public CreateRepairOrderUseCase(
        IAppDbContext context,
        INotificationService notificationService,
        IGetRepairOrderByIdUseCase getRepairOrderById)
    {
        _context = context;
        _notificationService = notificationService;
        _getRepairOrderById = getRepairOrderById;
    }

    public async Task<object> Execute(Guid businessId, CreateRepairOrderRequestDto request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var lineDtos = request.Items ?? new List<RepairOrderItemDto>();
        foreach (var it in lineDtos)
            RepairOrderItemsRequestHelper.ValidateLine(it);
        await RepairOrderItemsRequestHelper.ValidateVariantIdsForBusinessAsync(
            _context,
            businessId,
            lineDtos,
            CancellationToken.None);

        using var transaction = await ((DbContext)_context).Database.BeginTransactionAsync();

        var settings = await _context.BusinessSettings
            .FromSqlRaw(
                "SELECT * FROM \"BusinessSettings\" WHERE \"BusinessId\" = {0} FOR UPDATE",
                businessId)
            .FirstOrDefaultAsync();

        if (settings == null)
            throw new NotFoundException("BusinessSettings", "Business settings not found.");

        var orderNumber = await RepairOrderDailyNumberGenerator.GetNextForBusinessAndUtcDateAsync(
            _context.RepairOrders,
            businessId,
            DateTime.UtcNow,
            CancellationToken.None);

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

        var order = new RepairOrderEntity
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            OrderNumber = orderNumber,
            ContactId = contact.Id,
            Contact = contact,
            ProblemDescription = request.ProblemDescription,
            DeviceType = request.DeviceType,
            DeviceTypeOther = request.DeviceTypeOther,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            AccessoriesIncluded = request.AccessoriesIncluded,
            OperatingSystem = request.OperatingSystem,
            Password = request.Password,
            IsDiagnosticPaid = request.IsDiagnosticPaid,
            Status = (int)RepairOrderStatus.Pending,
            IsActive = true
        };

        _context.RepairOrders.Add(order);

        if (lineDtos.Count > 0)
        {
            var itemEntities = RepairOrderItemsRequestHelper.MapToNewEntities(order, lineDtos);
            _context.RepairOrderItems.AddRange(itemEntities);
        }

        await _context.SaveChangesAsync(CancellationToken.None);
        await transaction.CommitAsync();

        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null)
            throw new NotFoundException("Business", "Business not found.");

        await _notificationService.OrderCreatedAsync(business, order);

        var result = await _getRepairOrderById.Execute(order.Id);
        if (result == null)
            throw new InvalidOperationException("La orden creada no pudo leerse.");

        return result;
    }
}
