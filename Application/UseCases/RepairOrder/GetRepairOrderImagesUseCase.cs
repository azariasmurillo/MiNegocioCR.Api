using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.RepairOrders;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.UseCases.RepairOrder;

public class GetRepairOrderImagesUseCase : IGetRepairOrderImagesUseCase
{
    private readonly IAppDbContext _context;

    public GetRepairOrderImagesUseCase(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<RepairOrderImageDto>> ExecuteAsync(
        Guid businessId,
        Guid repairOrderId,
        CancellationToken cancellationToken = default)
    {
        if (businessId == Guid.Empty)
            throw new ArgumentException("BusinessId is required.", nameof(businessId));
        if (repairOrderId == Guid.Empty)
            throw new ArgumentException("RepairOrderId is required.", nameof(repairOrderId));

        var orderExists = await _context.RepairOrders
            .AnyAsync(o => o.Id == repairOrderId && o.BusinessId == businessId, cancellationToken);
        if (!orderExists)
            throw new NotFoundException("RepairOrder", "Repair order not found.");

        return await _context.RepairOrderImages
            .AsNoTracking()
            .Where(i => i.BusinessId == businessId && i.RepairOrderId == repairOrderId)
            .OrderBy(i => i.CreatedAt)
            .Select(i => new RepairOrderImageDto { Id = i.Id, ImageUrl = i.ImageUrl })
            .ToListAsync(cancellationToken);
    }
}
