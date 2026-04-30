using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

public interface IGetDashboardSummaryUseCase
{
    Task<DashboardSummaryDto> Execute(Guid businessId, DateTime? from, DateTime? to);
}
