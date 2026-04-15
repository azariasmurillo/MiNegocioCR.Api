using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IUpdateCatalogItemUseCase
    {
        Task ExecuteAsync(Guid id, UpdateCatalogItemRequestDto request);
    }
}
