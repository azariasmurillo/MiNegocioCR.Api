using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Business;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.Business;

public class UploadBusinessLogoUseCase : IUploadBusinessLogoUseCase
{
    private readonly IAppDbContext _context;
    private readonly IBusinessLogoStorageService _businessLogoStorageService;

    public UploadBusinessLogoUseCase(
        IAppDbContext context,
        IBusinessLogoStorageService businessLogoStorageService)
    {
        _context = context;
        _businessLogoStorageService = businessLogoStorageService;
    }

    public async Task<string> Execute(Guid businessId, Stream fileStream, string fileName)
    {
        if (businessId == Guid.Empty) throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));
        if (fileStream == null || !fileStream.CanRead) throw new ArgumentException("File stream is required.", nameof(fileStream));

        var business = await _context.Businesses.FirstOrDefaultAsync(x => x.Id == businessId);
        if (business == null)
            throw new NotFoundException("Business", "Business not found");

        var logoUrl = await _businessLogoStorageService.UploadLogoAsync(
            businessId,
            fileStream,
            fileName,
            CancellationToken.None);

        business.LogoUrl = logoUrl;
        await _context.SaveChangesAsync(CancellationToken.None);
        return logoUrl;
    }
}
