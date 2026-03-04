using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Aplication.Interfaces.RepairOrders
{
    public interface IGetRepairOrderByBusinessIdAndStatusUseCase
    {
        Task<List<object>> Execute(Guid businessId, RepairOrderStatus status);
    }
}
