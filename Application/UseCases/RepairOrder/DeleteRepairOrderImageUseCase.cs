using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

public class DeleteRepairOrderImageUseCase : IDeleteRepairOrderImageUseCase
{
    private readonly IAppDbContext _context;
    private readonly IRepairOrderImageStorageService _storage;

    public DeleteRepairOrderImageUseCase(
        IAppDbContext context,
        IRepairOrderImageStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task ExecuteAsync(Guid businessId, Guid imageId, CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (imageId == Guid.Empty)
            throw new ArgumentException("ImageId is required.", nameof(imageId));

        var image = await _context.RepairOrderImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.BusinessId == businessId, cancellationToken);

        if (image == null)
            throw new NotFoundException("RepairOrderImage", "Image not found.");

        await _storage.DeleteByPublicUrlAsync(image.ImageUrl, cancellationToken);

        _context.RepairOrderImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
