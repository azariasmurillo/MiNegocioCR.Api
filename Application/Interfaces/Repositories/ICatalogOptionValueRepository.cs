using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface ICatalogOptionValueRepository
    {
        Task AddAsync(CatalogOptionValue optionValue);

        Task<List<CatalogOptionValue>> GetByCatalogOptionIdAsync(Guid catalogOptionId);

        /// <summary>Carga valores por Id incluyendo la opción de catálogo asociada.</summary>
        Task<List<CatalogOptionValue>> GetByIdsWithCatalogOptionAsync(IReadOnlyList<Guid> ids);
    }
}
