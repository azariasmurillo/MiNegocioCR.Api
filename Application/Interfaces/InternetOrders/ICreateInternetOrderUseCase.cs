using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.InternetOrders;

public interface ICreateInternetOrderUseCase
{
    Task<object> Execute(Guid businessId, UpsertInternetOrderRequestDto request);
}
