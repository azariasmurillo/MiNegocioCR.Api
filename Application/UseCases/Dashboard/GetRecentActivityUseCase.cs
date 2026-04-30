using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.Application.UseCases.Dashboard;

public class GetRecentActivityUseCase : IGetRecentActivityUseCase
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetRecentActivityUseCase(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<List<ActivityItemDto>> Execute(Guid businessId, int? limit)
    {
        var safeLimit = limit.GetValueOrDefault(20);
        if (safeLimit <= 0) safeLimit = 20;
        if (safeLimit > 100) safeLimit = 100;
        return _dashboardRepository.GetRecentActivityAsync(businessId, safeLimit);
    }
}
