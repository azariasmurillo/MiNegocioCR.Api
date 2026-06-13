using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IGetBusinessDimensionValuesUseCase
    {
        Task<IReadOnlyList<BusinessDimensionValueDto>> ExecuteAsync(
            Guid businessId,
            string dimensionName,
            bool includeInactive = false);
    }

    public interface IGetCatalogDimensionCatalogUseCase
    {
        CatalogDimensionCatalogDto Execute();
    }
}
