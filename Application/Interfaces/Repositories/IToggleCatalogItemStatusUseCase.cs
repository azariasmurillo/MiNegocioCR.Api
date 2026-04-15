using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IToggleCatalogItemStatusUseCase
    {
        Task ExecuteAsync(Guid id, ToggleCatalogItemStatusRequestDto request);
    }
}
