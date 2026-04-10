using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICatalogRepository
    {
        Task<CatalogItem?> GetItemByIdAsync(Guid id);

        Task<CatalogItem?> GetItemAsync(Guid id, Guid businessId);

        Task<List<CatalogItem>> GetItemsAsync(Guid businessId);

        Task AddItemAsync(CatalogItem item);

        Task UpdateItemAsync(CatalogItem item);
    }
}
