using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders
{
    public interface IUpdateRepairOrderUseCase
    {
        Task Execute(Guid id, UpdateRepairOrderRequestDto request);
    }
}
