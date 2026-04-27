public interface IBusinessLogoStorageService
{
    Task<string> UploadLogoAsync(Guid businessId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
