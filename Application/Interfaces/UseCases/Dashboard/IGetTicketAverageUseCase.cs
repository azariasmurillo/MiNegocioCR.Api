using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.UseCases.Dashboard;

public interface IGetTicketAverageUseCase
{
    Task<TicketAverageDto> Execute(Guid businessId, DateTime? from, DateTime? to);
}
