using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Repositories;
using MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

namespace MiNegocioCR.Api.Application.UseCases.Dashboard;

public class GetSalesTrendUseCase : IGetSalesTrendUseCase
{
    private readonly IDashboardRepository _dashboardRepository;

    public GetSalesTrendUseCase(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<List<SalesTrendPointDto>> Execute(Guid businessId, DateTime? from, DateTime? to, string? groupBy)
    {
        var normalized = string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase) ? "month" : "day";
        return _dashboardRepository.GetSalesTrendAsync(businessId, from, to, normalized);
    }
}
