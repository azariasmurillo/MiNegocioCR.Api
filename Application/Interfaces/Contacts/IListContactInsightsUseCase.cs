using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Contacts;

public interface IListContactInsightsUseCase
{
    Task<ContactInsightsResultDto> Execute(
        Guid businessId,
        int inactiveDays = 60,
        bool? inactiveOnly = null,
        bool? hasEmailOnly = null,
        string? search = null);
}
