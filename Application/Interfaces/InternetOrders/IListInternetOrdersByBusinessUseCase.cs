namespace MiNegocioCR.Api.Application.Interfaces.InternetOrders;

public interface IListInternetOrdersByBusinessUseCase
{
    Task<List<object>> Execute(Guid businessId, string? statusFilter, string? search);
}
