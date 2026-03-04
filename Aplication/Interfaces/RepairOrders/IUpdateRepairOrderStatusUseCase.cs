using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.RepairOrders
{
    public interface IUpdateRepairOrderStatusUseCase
    {
        Task<object> Execute(Guid id, UpdateStatusRequestDto request);
    }
}
