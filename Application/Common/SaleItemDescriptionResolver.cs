using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.Common;

/// <summary>Etiqueta legible para líneas de venta/factura (nombre catálogo, SKU, opciones).</summary>
public static class SaleItemDescriptionResolver
{
    public static string? Resolve(string? storedDescription, CatalogVariant? variant)
    {
        if (!string.IsNullOrWhiteSpace(storedDescription))
            return storedDescription.Trim();

        if (variant == null)
            return null;

        return BuildFromVariant(variant);
    }

    public static string BuildFromVariant(CatalogVariant variant)
    {
        var parts = new List<string>();
        var name = variant.CatalogItem?.Name?.Trim();
        if (!string.IsNullOrEmpty(name))
            parts.Add(name);

        var sku = variant.SKU?.Trim();
        if (!string.IsNullOrEmpty(sku))
            parts.Add(sku);

        if (variant.VariantOptionValues != null)
        {
            foreach (var link in variant.VariantOptionValues
                         .OrderBy(l => l.CatalogOptionValue.CatalogOption.Name)
                         .ThenBy(l => l.CatalogOptionValue.Value))
            {
                var ov = link.CatalogOptionValue;
                parts.Add($"{ov.CatalogOption.Name}: {ov.Value}");
            }
        }

        return parts.Count > 0 ? string.Join(" | ", parts) : "Producto";
    }
}
