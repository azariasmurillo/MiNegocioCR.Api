using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICatalogOptionRepository
    {
        Task AddAsync(CatalogOption option);

        Task<CatalogOption?> GetByIdAsync(Guid id);

        Task<List<CatalogOption>> GetByCatalogItemIdAsync(Guid catalogItemId, bool includeInactive = false);

        Task UpdateAsync(CatalogOption option);

        Task<bool> ExistsWithValuesAsync(Guid optionId);

        /// <summary>True si alguna variante usa un valor de esta dimensión.</summary>
        Task<bool> ExistsInVariantsAsync(Guid optionId);

        Task DeleteAsync(CatalogOption option);
    }
}
