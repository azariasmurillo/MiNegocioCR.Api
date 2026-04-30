using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.Application.UseCases.Dashboard;

public class GetDashboardSummaryUseCase : IGetDashboardSummaryUseCase
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetDashboardSummaryUseCase(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<DashboardSummaryDto> Execute(Guid businessId, DateTime? from, DateTime? to)
    {
        return _dashboardRepository.GetSummaryAsync(businessId, from, to);
    }
}
