using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IGetOptionsByItemUseCase
    {
        Task<IReadOnlyList<CatalogOptionDto>> ExecuteAsync(Guid catalogItemId, bool includeInactive = false);
    }
}
