using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

public class GetRepairOrderBalanceUseCase : IGetRepairOrderBalanceUseCase
{
    private readonly IAppDbContext _context;
    private readonly IPaymentService _paymentService;

    public GetRepairOrderBalanceUseCase(IAppDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<RepairOrderBalanceDto> Execute(Guid businessId, Guid repairOrderId)
    {
        if (businessId == Guid.Empty) throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (repairOrderId == Guid.Empty) throw new ArgumentException("RepairOrderId is required.", nameof(repairOrderId));

        var order = await _context.RepairOrders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == repairOrderId && x.BusinessId == businessId);

        if (order is null)
            throw new NotFoundException("RepairOrder", "Repair order not found.");

        var business = await _context.Businesses.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == businessId);
        if (business == null)
            throw new NotFoundException("Business", "Business not found.");

        var taxRate = business.TaxRatePercent;
        if (taxRate < 0)
            throw new InvalidOperationException("Business tax rate cannot be negative.");

        var subtotal = order.Items?.Sum(x => x.Price * x.Quantity) ?? 0m;
        var discountPercentAmount = Math.Round(
            subtotal * (order.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);
        if (discountPercentAmount > subtotal)
            discountPercentAmount = subtotal;

        var taxableBase = subtotal - discountPercentAmount;
        var tax = Math.Round(taxableBase * (taxRate / 100m), 2, MidpointRounding.AwayFromZero);
        var totalOrden = taxableBase + tax;

        var totalPagado = await _paymentService.GetTotalPaidAsync(businessId, repairOrderId);
        var saldo = Math.Max(0m, totalOrden - totalPagado);

        return new RepairOrderBalanceDto
        {
            Subtotal = subtotal,
            Discount = discountPercentAmount,
            Tax = tax,
            Total = totalOrden,
            TotalPagado = totalPagado,
            SaldoPendiente = saldo
        };
    }
}
