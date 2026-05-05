namespace MiNegocioCR.Api.Application.Interfaces;

public interface IRepairOrderImageStorageService
{
    Task<string> UploadAsync(Guid repairOrderId, Stream fileStream, string contentType, CancellationToken cancellationToken = default);

    Task DeleteByPublicUrlAsync(string publicImageUrl, CancellationToken cancellationToken = default);
}
