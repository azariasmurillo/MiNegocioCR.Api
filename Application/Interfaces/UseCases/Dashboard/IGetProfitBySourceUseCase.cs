using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

public interface IGetProfitBySourceUseCase
{
    Task<ProfitBySourceDto> Execute(Guid businessId);
}
