using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders
{
    public interface IUpdateRepairOrderUseCase
    {
        Task Execute(Guid id, UpdateRepairOrderRequestDto request);
    }
}
