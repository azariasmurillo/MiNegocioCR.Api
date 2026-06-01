namespace MiNegocioCR.Api.Application.Interfaces.Services;

public interface ICampaignImageStorageService
{
    Task<string> UploadAsync(
        Guid businessId,
        Stream fileStream,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default);
}
