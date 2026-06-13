using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICatalogOptionValueRepository
    {
        Task AddAsync(CatalogOptionValue optionValue);

        Task<CatalogOptionValue?> GetByIdAsync(Guid id);

        Task<List<CatalogOptionValue>> GetByCatalogOptionIdAsync(Guid catalogOptionId, bool includeInactive = false);

        Task<bool> ExistsActiveValueAsync(Guid catalogOptionId, string normalizedValue, Guid? excludeValueId = null);

        Task<List<CatalogOptionValue>> GetByIdsWithCatalogOptionAsync(IReadOnlyList<Guid> ids);

        Task UpdateAsync(CatalogOptionValue optionValue);

        Task<bool> ExistsInVariantsAsync(Guid optionValueId);

        Task DeleteAsync(CatalogOptionValue optionValue);
    }
}
