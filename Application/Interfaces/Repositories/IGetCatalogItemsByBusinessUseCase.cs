using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IGetCatalogItemsByBusinessUseCase
    {
        Task<IReadOnlyList<CatalogItemDto>> ExecuteAsync(Guid businessId, bool includeInactive = false);
    }
}
