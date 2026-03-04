using MiNegocioCR.Api.Aplication.DTOs;

namespace MiNegocioCR.Api.Aplication.Interfaces.RepairOrders
{
    public interface ICreateRepairOrderUseCase
    {
        Task<object> Execute(Guid businessId, CreateRepairOrderRequestDto request);
    }
}
