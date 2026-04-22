using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IUpdateVariantUseCase
    {
        Task ExecuteAsync(Guid variantId, UpdateVariantRequestDto request);
    }
}
