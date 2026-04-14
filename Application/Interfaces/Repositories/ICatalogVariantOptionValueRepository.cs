using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICatalogVariantOptionValueRepository
    {
        /// <summary>Inserta una relación variante–valor de opción.</summary>
        Task AddAsync(CatalogVariantOptionValue link);

        /// <summary>Inserta varias relaciones en una sola transacción (misma combinación o varias filas).</summary>
        Task AddRangeAsync(IReadOnlyList<CatalogVariantOptionValue> links);

        /// <summary>
        /// Indica si ya existe una variante del ítem con exactamente el mismo conjunto de valores de opción
        /// (<paramref name="sortedDistinctOptionValueIds"/> ordenado, sin duplicados).
        /// </summary>
        Task<bool> ExistsVariantWithSameOptionValueCombinationAsync(
            Guid catalogItemId,
            IReadOnlyList<Guid> sortedDistinctOptionValueIds);
    }
}
