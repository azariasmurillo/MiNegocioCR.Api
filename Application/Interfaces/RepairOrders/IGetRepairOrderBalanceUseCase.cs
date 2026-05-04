using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders;

public interface IGetRepairOrderBalanceUseCase
{
    Task<RepairOrderBalanceDto> Execute(Guid businessId, Guid repairOrderId);
}
