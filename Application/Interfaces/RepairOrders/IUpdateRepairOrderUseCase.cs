using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders
{
    public interface IUpdateRepairOrderUseCase
    {
        Task<object> Execute(Guid businessId, Guid id, UpdateRepairOrderRequestDto request);
    }
}
