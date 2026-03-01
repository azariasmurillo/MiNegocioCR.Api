namespace MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders
{
    public interface IGetRepairOrdersByBusinessUseCase
    {
        Task<List<object>> Execute(Guid businessId);
    }
}
