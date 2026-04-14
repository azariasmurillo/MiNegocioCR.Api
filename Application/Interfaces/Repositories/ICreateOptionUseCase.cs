using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICreateOptionUseCase
    {
        Task<Guid> ExecuteAsync(CreateCatalogOptionRequestDto request);
    }
}
