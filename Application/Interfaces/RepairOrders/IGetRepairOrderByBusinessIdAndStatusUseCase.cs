using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders
{
    public interface IGetRepairOrderByBusinessIdAndStatusUseCase
    {
        Task<List<object>> Execute(Guid businessId, RepairOrderStatus status);
    }
}
