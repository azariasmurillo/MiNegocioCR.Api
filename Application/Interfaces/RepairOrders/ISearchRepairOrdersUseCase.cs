namespace MiNegocioCR.Api.Application.Interfaces.RepairOrders;

public interface ISearchRepairOrdersUseCase
{
    Task<List<object>> Execute(Guid businessId, string? query, DateTime? fromUtc, DateTime? toUtc);
}
