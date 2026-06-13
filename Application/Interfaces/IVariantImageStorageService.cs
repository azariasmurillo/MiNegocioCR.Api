namespace MiNegocioCR.Api.Application.Interfaces;

public interface IVariantImageStorageService
{
    Task<string> UploadAsync(
        Guid catalogVariantId,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<ProcessedVariantImageUrls> UploadProcessedAsync(
        Guid catalogVariantId,
        Guid imageId,
        ProcessedVariantImageStreams files,
        CancellationToken cancellationToken = default);

    Task DeleteByPublicUrlAsync(string publicImageUrl, CancellationToken cancellationToken = default);
}

public sealed class ProcessedVariantImageStreams
{
    public required Stream Main { get; init; }

    public required Stream Mobile { get; init; }

    public required Stream Thumbnail { get; init; }
}

public sealed class ProcessedVariantImageUrls
{
    public required string MainUrl { get; init; }

    public required string MobileUrl { get; init; }

    public required string ThumbnailUrl { get; init; }
}
