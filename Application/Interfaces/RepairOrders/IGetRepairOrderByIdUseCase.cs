namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders
{
    public interface IGetRepairOrderByIdUseCase
    {
        Task<object?> Execute(Guid businessId, Guid id);
    }
}
