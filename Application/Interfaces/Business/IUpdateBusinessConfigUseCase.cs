using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Business;

public interface IUpdateBusinessConfigUseCase
{
    Task<BusinessConfigDto> Execute(Guid businessId, UpdateBusinessConfigRequestDto request);
}
