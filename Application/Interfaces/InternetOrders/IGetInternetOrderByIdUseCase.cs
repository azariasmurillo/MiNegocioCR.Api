namespace MiNegocioCR.Api.Application.Interfaces.InternetOrders;

public interface IGetInternetOrderByIdUseCase
{
    Task<object?> Execute(Guid businessId, Guid id);
}
