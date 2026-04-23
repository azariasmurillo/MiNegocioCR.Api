using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Application.Common;

public static class RepairOrderItemsRequestHelper
{
    public static void ValidateLine(RepairOrderItemDto dto)
    {
        if (dto.Quantity <= 0)
            throw new ArgumentException("Cada ítem debe tener una cantidad mayor a cero.", nameof(dto.Quantity));
        if (dto.Price < 0)
            throw new ArgumentException("El precio de cada ítem no puede ser negativo.", nameof(dto.Price));
        if (!dto.CatalogVariantId.HasValue && string.IsNullOrWhiteSpace(dto.Description))
            throw new ArgumentException(
                "Cada ítem sin variante de catálogo requiere descripción (texto libre).", nameof(dto.Description));
    }

    public static async Task ValidateVariantIdsForBusinessAsync(
        IAppDbContext context,
        Guid businessId,
        IReadOnlyList<RepairOrderItemDto> items,
        CancellationToken cancellationToken = default)
    {
        var ids = items
            .Where(x => x.CatalogVariantId.HasValue)
            .Select(x => x.CatalogVariantId!.Value)
            .Distinct()
            .ToList();
        if (ids.Count == 0) return;

        var valid = await context.CatalogVariants
            .AsNoTracking()
            .Where(v => ids.Contains(v.Id) && v.CatalogItem.BusinessId == businessId)
            .Select(v => v.Id)
            .ToListAsync(cancellationToken);
        if (valid.Count != ids.Count)
        {
            throw new NotFoundException(
                "CatalogVariant",
                "Una o más variantes no existen o no pertenecen a este negocio.");
        }
    }

    public static IReadOnlyList<RepairOrderItem> MapToNewEntities(RepairOrder order, IEnumerable<RepairOrderItemDto> dtos) =>
        dtos.Select(d => new RepairOrderItem
        {
            Id = Guid.NewGuid(),
            RepairOrderId = order.Id,
            CatalogVariantId = d.CatalogVariantId,
            Description = string.IsNullOrWhiteSpace(d.Description) ? null : d.Description.Trim(),
            Quantity = d.Quantity,
            Price = d.Price
        }).ToList();
}
