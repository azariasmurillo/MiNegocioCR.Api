using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICreateCategoryUseCase
    {
        Task<Guid> ExecuteAsync(CreateCategoryRequestDto request);
    }
}
