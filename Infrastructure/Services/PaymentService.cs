using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Services;
using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IAppDbContext _context;

    public PaymentService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetTotalPaidAsync(Guid businessId, Guid repairOrderId)
    {
        return await _context.Payments
            .Where(x => x.BusinessId == businessId && x.RepairOrderId == repairOrderId)
            .Select(x => (decimal?)x.Amount)
            .SumAsync() ?? 0m;
    }

    public async Task<List<Payment>> GetPaymentsByRepairOrderAsync(Guid businessId, Guid repairOrderId)
    {
        return await _context.Payments
            .Where(x => x.BusinessId == businessId && x.RepairOrderId == repairOrderId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
}
