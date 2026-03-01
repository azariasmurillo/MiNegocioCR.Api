using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders
{
    public interface ICreateRepairOrderUseCase
    {
        Task<object> Execute(Guid businessId, CreateRepairOrderRequestDto request);
    }
}
