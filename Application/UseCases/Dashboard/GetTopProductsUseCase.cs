using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.Application.UseCases.Dashboard;

public class GetTopProductsUseCase : IGetTopProductsUseCase
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetTopProductsUseCase(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<List<TopProductRowDto>> Execute(Guid businessId, int take = 10)
    {
        return _dashboardRepository.GetTopProductsAsync(businessId, take);
    }
}
