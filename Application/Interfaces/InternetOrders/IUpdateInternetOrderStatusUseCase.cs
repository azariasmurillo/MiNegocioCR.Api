using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.InternetOrders;

public interface IUpdateInternetOrderStatusUseCase
{
    Task<object> Execute(Guid businessId, Guid id, UpdateInternetOrderStatusRequestDto request);
}
