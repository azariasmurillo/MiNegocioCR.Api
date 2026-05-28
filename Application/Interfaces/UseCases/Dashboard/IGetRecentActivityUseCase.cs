using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

public interface IGetRecentActivityUseCase
{
    Task<List<ActivityItemDto>> Execute(Guid businessId, int? limit, DateTime? fromUtcInclusive, DateTime? toUtcExclusive);
}
