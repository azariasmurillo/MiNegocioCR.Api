using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICatalogCategoryRepository
    {
        Task AddAsync(CatalogCategory category);

        Task<List<CatalogCategory>> GetByBusinessIdAsync(Guid businessId);
    }
}
