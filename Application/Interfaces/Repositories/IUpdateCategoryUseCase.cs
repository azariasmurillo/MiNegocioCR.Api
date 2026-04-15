using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IUpdateCategoryUseCase
    {
        Task ExecuteAsync(Guid categoryId, UpdateCategoryRequestDto request);
    }
}
