using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IUpdateOptionUseCase
    {
        Task ExecuteAsync(Guid id, UpdateOptionRequestDto request);
    }
}
