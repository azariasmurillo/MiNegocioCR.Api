using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.Application.UseCases.Dashboard;

public class GetPendingOrdersDashboardUseCase : IGetPendingOrdersDashboardUseCase
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetPendingOrdersDashboardUseCase(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<List<PendingOrderRowDto>> Execute(Guid businessId)
    {
        return _dashboardRepository.GetPendingOrdersWithBalanceAsync(businessId);
    }
}
