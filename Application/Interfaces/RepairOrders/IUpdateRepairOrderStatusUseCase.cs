using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders
{
    public interface IUpdateRepairOrderStatusUseCase
    {
        Task<object> Execute(Guid id, UpdateStatusRequestDto request);
    }
}
