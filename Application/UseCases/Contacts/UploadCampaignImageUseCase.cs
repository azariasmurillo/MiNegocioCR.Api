using MiNegocioCR.Api.Application.Common;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Contacts;
using MiNegocioCR.Api.Application.Interfaces.Services;

namespace MiNegocioCR.Api.Application.UseCases.Contacts;

public class UploadCampaignImageUseCase : IUploadCampaignImageUseCase
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
    };

    private readonly ICampaignImageStorageService _storage;

    public UploadCampaignImageUseCase(ICampaignImageStorageService storage)
    {
        _storage = storage;
    }

    public async Task<UploadCampaignImageResultDto> Execute(
        Guid businessId,
        Stream fileStream,
        long fileLength,
        string fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (fileStream == null || fileLength <= 0)
            throw new ArgumentException("Image file is required.");
        if (fileLength > CampaignImageLimits.MaxUploadBytes)
            throw new ArgumentException($"La imagen debe ser menor a {CampaignImageLimits.MaxUploadLabel}.");
        if (!string.IsNullOrWhiteSpace(contentType) && !AllowedContentTypes.Contains(contentType))
            throw new ArgumentException("Formato de imagen no permitido. Usá JPG, PNG, WEBP o GIF.");

        await using var buffer = new MemoryStream((int)Math.Min(fileLength, int.MaxValue));
        await fileStream.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length == 0)
            throw new ArgumentException("Image file is required.");

        var optimized = await CampaignImageProcessor.OptimizeFromBytesAsync(buffer.ToArray(), cancellationToken);
        await using (optimized.Output)
        {
            var url = await _storage.UploadAsync(
                businessId,
                optimized.Output,
                $"{Path.GetFileNameWithoutExtension(fileName)}{optimized.FileExtension}",
                optimized.ContentType,
                cancellationToken);
            return new UploadCampaignImageResultDto { ImageUrl = url };
        }
    }
}
