using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.InternetOrders;

public interface IUpdateInternetOrderUseCase
{
    Task<object> Execute(Guid businessId, Guid id, UpsertInternetOrderRequestDto request);
}
