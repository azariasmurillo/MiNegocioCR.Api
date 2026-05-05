using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Variants;

public interface IGetCatalogVariantImagesUseCase
{
    Task<IReadOnlyList<CatalogVariantImageDto>> ExecuteAsync(
        Guid businessId,
        Guid catalogVariantId,
        CancellationToken cancellationToken = default);
}
