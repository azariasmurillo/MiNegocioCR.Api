using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

public interface IGetTopProductsUseCase
{
    Task<List<TopProductRowDto>> Execute(Guid businessId, int take = 10);
}
