namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders
{
    public interface IGetRepairOrdersByBusinessUseCase
    {
        Task<List<object>> Execute(Guid businessId);
    }
}
