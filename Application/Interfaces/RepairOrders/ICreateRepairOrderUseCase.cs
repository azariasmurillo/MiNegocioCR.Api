using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders
{
    public interface ICreateRepairOrderUseCase
    {
        Task<object> Execute(Guid businessId, CreateRepairOrderRequestDto request);
    }
}
