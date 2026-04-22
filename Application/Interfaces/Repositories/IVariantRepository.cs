using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IVariantRepository
    {
        Task<CatalogVariant?> GetVariantAsync(Guid variantId, Guid businessId);

        Task<List<CatalogVariant>> GetVariantsByItemAsync(Guid catalogItemId);

        /// <summary>Variantes del ítem con opciones y nombres para listados (sin tracking).</summary>
        Task<List<CatalogVariant>> GetVariantsWithOptionDetailsByCatalogItemIdAsync(Guid catalogItemId);

        /// <summary>Variantes del negocio con opciones y nombres para listados (sin tracking).</summary>
        Task<List<CatalogVariant>> GetVariantsWithOptionDetailsByBusinessAsync(
            Guid businessId,
            Guid? catalogItemId = null,
            string? search = null);

        /// <summary>Cantidades del movimiento de stock inicial (Purchase + nota estándar) por variante.</summary>
        Task<Dictionary<Guid, int>> GetInitialStockQuantitiesAsync(IReadOnlyCollection<Guid> variantIds);

        Task AddVariantAsync(CatalogVariant variant);

        Task UpdateAsync(CatalogVariant variant);

        Task DeleteAsync(CatalogVariant variant);

        /// <summary>True si hay movimientos distintos al stock inicial de creación (Purchase + nota estándar).</summary>
        Task<bool> ExistsInInventoryAsync(Guid variantId);

        Task<bool> ExistsInSalesAsync(Guid variantId);

        Task<bool> ExistsInPurchasesAsync(Guid variantId);
    }
}
