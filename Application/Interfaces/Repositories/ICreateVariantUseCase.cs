using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICreateVariantUseCase
    {
        Task<Guid> ExecuteAsync(CreateVariantRequestDto request);
    }
}
