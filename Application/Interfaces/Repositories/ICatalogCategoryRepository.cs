using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICatalogCategoryRepository
    {
        Task AddAsync(CatalogCategory category);

        Task<CatalogCategory?> GetByIdAsync(Guid id);

        Task<List<CatalogCategory>> GetByBusinessIdAsync(Guid businessId, bool includeInactive = false);

        Task UpdateAsync(CatalogCategory category);

        Task<bool> ExistsWithProductsAsync(Guid categoryId);
    }
}
