using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IVariantRepository
    {
        Task<CatalogVariant?> GetVariantAsync(Guid variantId, Guid businessId);

        Task<List<CatalogVariant>> GetVariantsByItemAsync(Guid catalogItemId);

        Task AddVariantAsync(CatalogVariant variant);

        Task UpdateVariantAsync(CatalogVariant variant);
    }
}
