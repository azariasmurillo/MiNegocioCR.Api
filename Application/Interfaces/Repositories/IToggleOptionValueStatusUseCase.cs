using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IToggleOptionValueStatusUseCase
    {
        Task ExecuteAsync(Guid id, ToggleOptionValueStatusRequestDto request);
    }
}
