namespace MiNegocioCR.Api.Aplication.Interfaces.RepairOrders
{
    public interface IGetRepairOrderByIdUseCase
    {
        Task<object?> Execute(Guid id);
    }
}
