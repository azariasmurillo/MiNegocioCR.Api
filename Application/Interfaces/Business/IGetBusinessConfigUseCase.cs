using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Business;

public interface IGetBusinessConfigUseCase
{
    Task<BusinessConfigDto?> Execute(Guid businessId);
}
