using MiNegocioCR.Api.Application.DTOs;

namespace MiNegocioCR.Api.Application.Interfaces.Variants;

public interface IStartVariantImageImportZipUseCase
{
    Task<Guid> ExecuteAsync(StartVariantImageImportZipInput input, CancellationToken cancellationToken = default);
}

public interface IGetImageImportBatchUseCase
{
    Task<ImageImportBatchDto> ExecuteAsync(Guid businessId, Guid batchId, CancellationToken cancellationToken = default);
}

public interface IGetImageImportBatchLogsUseCase
{
    Task<IReadOnlyList<ImageImportLogDto>> ExecuteAsync(
        Guid businessId,
        Guid batchId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

public interface IImageImportBatchProcessor
{
    Task<bool> ProcessNextAsync(CancellationToken cancellationToken = default);
}

public sealed class StartVariantImageImportZipInput
{
    public required Guid BusinessId { get; init; }

    public required Guid CreatedByUserId { get; init; }

    public required Stream ZipStream { get; init; }

    public required string OriginalFileName { get; init; }

    public required long ZipLength { get; init; }

    public bool ReplaceExisting { get; init; }

    public bool UseBackgroundRemoval { get; init; }

    public string MarketplaceStyle { get; init; } = "marketplace-white-v1";
}
