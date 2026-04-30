using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.Application.UseCases.Dashboard;

public class GetTicketAverageUseCase : IGetTicketAverageUseCase
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetTicketAverageUseCase(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<TicketAverageDto> Execute(Guid businessId, DateTime? from, DateTime? to)
    {
        return _dashboardRepository.GetTicketAverageAsync(businessId, from, to);
    }
}
