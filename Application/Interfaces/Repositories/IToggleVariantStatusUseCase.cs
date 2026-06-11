using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IToggleVariantStatusUseCase
    {
        Task ExecuteAsync(Guid variantId, ToggleVariantStatusRequestDto request);
    }
}
