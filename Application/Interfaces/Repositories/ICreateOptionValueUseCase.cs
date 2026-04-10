using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICreateOptionValueUseCase
    {
        Task<Guid> ExecuteAsync(CreateCatalogOptionValueRequestDto request);
    }
}
