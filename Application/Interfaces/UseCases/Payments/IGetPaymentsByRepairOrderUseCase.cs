using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Payments;

public interface IGetPaymentsByRepairOrderUseCase
{
    Task<List<PaymentItemDto>> Execute(Guid businessId, Guid repairOrderId);
}
