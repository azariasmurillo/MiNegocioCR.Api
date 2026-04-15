using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IToggleOptionStatusUseCase
    {
        Task ExecuteAsync(Guid id, ToggleOptionStatusRequestDto request);
    }
}
