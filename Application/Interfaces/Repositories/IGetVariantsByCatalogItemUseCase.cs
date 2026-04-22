using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IGetVariantsByCatalogItemUseCase
    {
        Task<List<CatalogVariantListItemDto>> ExecuteAsync(Guid catalogItemId, Guid businessId);
    }
}
