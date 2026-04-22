using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Repositories
{
    public interface IGetVariantsByBusinessUseCase
    {
        Task<List<CatalogVariantListItemDto>> ExecuteAsync(
            Guid businessId,
            Guid? catalogItemId = null,
            string? search = null);
    }
}
