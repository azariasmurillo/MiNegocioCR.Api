using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders;

public interface IChargeRepairOrderUseCase
{
    Task<object> Execute(Guid businessId, Guid repairOrderId, List<SalePaymentMethodDto>? paymentMethods = null);
}
