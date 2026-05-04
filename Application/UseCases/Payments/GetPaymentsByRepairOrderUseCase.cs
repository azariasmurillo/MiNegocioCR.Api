using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Payments;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Payments;

public class GetPaymentsByRepairOrderUseCase : IGetPaymentsByRepairOrderUseCase
{
    private readonly IAppDbContext _context;
    private readonly IPaymentService _paymentService;

    public GetPaymentsByRepairOrderUseCase(IAppDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<List<PaymentItemDto>> Execute(Guid businessId, Guid repairOrderId)
    {
        if (businessId == Guid.Empty) throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (repairOrderId == Guid.Empty) throw new ArgumentException("RepairOrderId is required.", nameof(repairOrderId));

        var exists = await _context.RepairOrders
            .AnyAsync(x => x.Id == repairOrderId && x.BusinessId == businessId);
        if (!exists)
            throw new NotFoundException("RepairOrder", "Repair order not found.");

        var payments = await _paymentService.GetPaymentsByRepairOrderAsync(businessId, repairOrderId);
        return payments.Select(x => new PaymentItemDto
        {
            Id = x.Id,
            Amount = x.Amount,
            Type = x.Type,
            Method = x.Method,
            CreatedAt = x.CreatedAt
        }).ToList();
    }
}
