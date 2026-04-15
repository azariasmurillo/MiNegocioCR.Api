using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IGetCategoriesByBusinessUseCase
    {
        Task<IReadOnlyList<CatalogCategoryDto>> ExecuteAsync(Guid businessId, bool includeInactive = false);
    }
}
