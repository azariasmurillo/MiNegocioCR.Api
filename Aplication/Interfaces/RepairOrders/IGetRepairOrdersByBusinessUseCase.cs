namespace MiNegocioCR.Api.Aplication.Interfaces.RepairOrders
{
    public interface IGetRepairOrdersByBusinessUseCase
    {
        Task<List<object>> Execute(Guid businessId);
    }
}
