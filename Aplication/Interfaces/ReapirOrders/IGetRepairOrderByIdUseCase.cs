namespace MiNegocioCR.Api.Aplication.Interfaces.ReapirOrders
{
    public interface IGetRepairOrderByIdUseCase
    {
        Task<object?> Execute(Guid id);
    }
}
