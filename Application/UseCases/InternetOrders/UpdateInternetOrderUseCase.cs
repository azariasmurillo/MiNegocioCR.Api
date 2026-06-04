using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.InternetOrders;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.InternetOrders;

public class UpdateInternetOrderUseCase : IUpdateInternetOrderUseCase
{
    private readonly IAppDbContext _context;
    private readonly IGetInternetOrderByIdUseCase _getById;

    public UpdateInternetOrderUseCase(IAppDbContext context, IGetInternetOrderByIdUseCase getById)
    {
        _context = context;
        _getById = getById;
    }

    public async Task<object> Execute(Guid businessId, Guid id, UpsertInternetOrderRequestDto request)
    {
        InternetOrderPersistenceHelper.ValidateUpsertRequest(request);

        var order = await _context.InternetOrders
            .AsTracking()
            .Include(o => o.Lines)
            .Include(o => o.Advances)
            .FirstOrDefaultAsync(o => o.BusinessId == businessId && o.Id == id);

        if (order == null)
            throw new NotFoundException("InternetOrder", "Pedido no encontrado.");

        var contact = await RepairOrderContactHelper.ResolveContactForCreateAsync(
            _context,
            businessId,
            request.ContactId,
            request.CustomerName,
            request.CustomerPhone,
            request.CustomerEmail);

        order.ContactId = contact.Id;

        _context.InternetOrderLines.RemoveRange(order.Lines);
        _context.InternetOrderAdvances.RemoveRange(order.Advances);
        order.Lines.Clear();
        order.Advances.Clear();

        InternetOrderPersistenceHelper.ApplyMetadataFromRequest(order, request);
        InternetOrderPersistenceHelper.ApplyTotalsToOrder(
            order,
            request.ExchangeRateApplied,
            request.InternationalShippingCost,
            request.LocalCourierCost,
            request.ServiceFee,
            request.Lines,
            request.Advances);

        var newLines = InternetOrderPersistenceHelper.BuildLines(order.Id, request.ExchangeRateApplied, request.Lines);
        var newAdvances = InternetOrderPersistenceHelper.BuildAdvances(order.Id, request.Advances);
        _context.InternetOrderLines.AddRange(newLines);
        _context.InternetOrderAdvances.AddRange(newAdvances);

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _getById.Execute(businessId, id);
        if (result == null)
            throw new InvalidOperationException("El pedido actualizado no pudo leerse.");

        return result;
    }
}
