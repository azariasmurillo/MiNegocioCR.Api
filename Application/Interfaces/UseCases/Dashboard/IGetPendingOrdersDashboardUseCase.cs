using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

public interface IGetPendingOrdersDashboardUseCase
{
    Task<List<PendingOrderRowDto>> Execute(Guid businessId);
}
