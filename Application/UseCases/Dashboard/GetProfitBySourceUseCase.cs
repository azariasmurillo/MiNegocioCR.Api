using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.Application.UseCases.Dashboard;

public class GetProfitBySourceUseCase : IGetProfitBySourceUseCase
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetProfitBySourceUseCase(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<ProfitBySourceDto> Execute(Guid businessId)
    {
        return _dashboardRepository.GetProfitBySourceAsync(businessId);
    }
}
