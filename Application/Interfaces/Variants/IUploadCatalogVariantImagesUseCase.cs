using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Variants;

public interface IUploadCatalogVariantImagesUseCase
{
    Task<IReadOnlyList<CatalogVariantImageDto>> ExecuteAsync(
        Guid businessId,
        Guid catalogVariantId,
        IReadOnlyList<CatalogVariantImageUploadInput> files,
        CancellationToken cancellationToken = default);
}
