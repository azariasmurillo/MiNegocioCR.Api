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

        Task DeleteAsync(CatalogOption option);
    }
}
