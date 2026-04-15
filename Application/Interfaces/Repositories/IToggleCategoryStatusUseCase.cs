using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IToggleCategoryStatusUseCase
    {
        Task ExecuteAsync(Guid categoryId, ToggleCategoryStatusRequestDto request);
    }
}
