namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders;

public interface IChargeRepairOrderUseCase
{
    Task<object> Execute(Guid businessId, Guid repairOrderId);
}
