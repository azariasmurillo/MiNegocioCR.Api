using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IUpdateOptionValueUseCase
    {
        Task ExecuteAsync(Guid id, UpdateOptionValueRequestDto request);
    }
}
