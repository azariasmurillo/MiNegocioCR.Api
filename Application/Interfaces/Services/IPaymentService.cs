using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<decimal> GetTotalPaidAsync(Guid businessId, Guid repairOrderId);
    Task<List<Payment>> GetPaymentsByRepairOrderAsync(Guid businessId, Guid repairOrderId);
}
