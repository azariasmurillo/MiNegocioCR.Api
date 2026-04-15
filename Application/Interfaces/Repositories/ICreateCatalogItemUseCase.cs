using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICreateCatalogItemUseCase
    {
        Task<Guid> ExecuteAsync(CreateCatalogItemRequestDto request);
    }
}
