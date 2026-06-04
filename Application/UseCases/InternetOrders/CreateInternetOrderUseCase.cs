using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.InternetOrders;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.InternetOrders;

public class CreateInternetOrderUseCase : ICreateInternetOrderUseCase
{
    private readonly IAppDbContext _context;
    private readonly IGetInternetOrderByIdUseCase _getById;

    public CreateInternetOrderUseCase(IAppDbContext context, IGetInternetOrderByIdUseCase getById)
    {
        _context = context;
        _getById = getById;
    }

    public async Task<object> Execute(Guid businessId, UpsertInternetOrderRequestDto request)
    {
        InternetOrderPersistenceHelper.ValidateUpsertRequest(request);

        using var transaction = await ((DbContext)_context).Database.BeginTransactionAsync();

        var orderNumber = await InternetOrderDailyNumberGenerator.GetNextForBusinessAndUtcDateAsync(
            _context.InternetOrders,
            businessId,
            DateTime.UtcNow,
            CancellationToken.None);

        var contact = await RepairOrderContactHelper.ResolveContactForCreateAsync(
            _context,
            businessId,
            request.ContactId,
            request.CustomerName,
            request.CustomerPhone,
            request.CustomerEmail);

        var orderId = Guid.NewGuid();
        var order = new InternetOrder
        {
            Id = orderId,
            BusinessId = businessId,
            ContactId = contact.Id,
            OrderNumber = orderNumber,
            Status = (int)InternetOrderStatus.Created,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        InternetOrderPersistenceHelper.ApplyMetadataFromRequest(order, request);
        InternetOrderPersistenceHelper.ApplyTotalsToOrder(
            order,
            request.ExchangeRateApplied,
            request.InternationalShippingCost,
            request.LocalCourierCost,
            request.ServiceFee,
            request.Lines,
            request.Advances);

        _context.InternetOrders.Add(order);
        _context.InternetOrderLines.AddRange(
            InternetOrderPersistenceHelper.BuildLines(orderId, request.ExchangeRateApplied, request.Lines));
        _context.InternetOrderAdvances.AddRange(
            InternetOrderPersistenceHelper.BuildAdvances(orderId, request.Advances));

        await _context.SaveChangesAsync(CancellationToken.None);
        await transaction.CommitAsync();

        var result = await _getById.Execute(businessId, orderId);
        if (result == null)
            throw new InvalidOperationException("El pedido creado no pudo leerse.");

        return result;
    }
}
