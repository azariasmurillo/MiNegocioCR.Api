namespace MiNegocioCR.Api.Application.Interfaces;

public interface IVariantImageStorageService
{
    Task<string> UploadAsync(
        Guid catalogVariantId,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteByPublicUrlAsync(string publicImageUrl, CancellationToken cancellationToken = default);
}
