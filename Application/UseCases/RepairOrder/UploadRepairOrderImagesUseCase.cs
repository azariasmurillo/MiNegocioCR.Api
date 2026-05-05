using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

public class UploadRepairOrderImagesUseCase : IUploadRepairOrderImagesUseCase
{
    private readonly IAppDbContext _context;
    private readonly IRepairOrderImageStorageService _storage;

    public UploadRepairOrderImagesUseCase(
        IAppDbContext context,
        IRepairOrderImageStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task<IReadOnlyList<RepairOrderImageDto>> ExecuteAsync(
        Guid businessId,
        Guid repairOrderId,
        IReadOnlyList<RepairOrderImageUploadInput> files,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (repairOrderId == Guid.Empty)
            throw new ArgumentException("RepairOrderId is required.", nameof(repairOrderId));
        if (files == null || files.Count == 0)
            throw new ArgumentException("At least one file is required.", nameof(files));

        var orderExists = await _context.RepairOrders
            .AnyAsync(o => o.Id == repairOrderId && o.BusinessId == businessId, cancellationToken);
        if (!orderExists)
            throw new NotFoundException("RepairOrder", "Repair order not found.");

        var results = new List<RepairOrderImageDto>(files.Count);

        foreach (var file in files)
        {
            var imageUrl = await _storage.UploadAsync(repairOrderId, file.Stream, file.ContentType, cancellationToken);

            var entity = new RepairOrderImage
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                RepairOrderId = repairOrderId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.RepairOrderImages.Add(entity);
            results.Add(new RepairOrderImageDto { Id = entity.Id, ImageUrl = entity.ImageUrl });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return results;
    }
}
