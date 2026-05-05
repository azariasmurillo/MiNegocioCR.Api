using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Variants;

public interface ISetPrimaryCatalogVariantImageUseCase
{
    Task<IReadOnlyList<CatalogVariantImageDto>> ExecuteAsync(
        Guid businessId,
        Guid imageId,
        CancellationToken cancellationToken = default);
}
