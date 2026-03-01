using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders
{
    public interface IUpdateRepairOrderStatusUseCase
    {
        Task<object> Execute(Guid id, UpdateStatusRequestDto request);
    }
}
