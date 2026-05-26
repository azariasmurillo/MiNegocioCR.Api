using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

/// <summary>Proyección de líneas de orden para respuestas API (GET detalle, listado, búsqueda).</summary>
public static class RepairOrderItemProjection
{
    public static object Map(RepairOrderItem i) => new
    {
        i.Id,
        i.CatalogVariantId,
        CatalogItemType = i.CatalogVariant != null ? (int?)i.CatalogVariant.CatalogItem.Type : null,
        Description = ResolveDescription(i),
        i.Quantity,
        i.Price
    };

    public static string? ResolveDescription(RepairOrderItem i)
    {
        if (!string.IsNullOrWhiteSpace(i.Description))
            return i.Description.Trim();

        if (i.CatalogVariant == null)
            return null;

        var name = i.CatalogVariant.CatalogItem?.Name?.Trim();
        var sku = i.CatalogVariant.SKU?.Trim();
        if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(sku))
            return null;
        if (string.IsNullOrEmpty(sku))
            return name;
        if (string.IsNullOrEmpty(name))
            return sku;
        return $"{name} | {sku}";
    }
}
