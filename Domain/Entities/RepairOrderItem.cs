namespace MiNegocioCR.Api.Domain.Entities;

public class RepairOrderItem
{
    public Guid Id { get; set; }

    public Guid RepairOrderId { get; set; }

    /// <summary>Variante de catálogo/inventario; null si es línea solo descriptiva.</summary>
    public Guid? CatalogVariantId { get; set; }

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public RepairOrder RepairOrder { get; set; } = null!;
    public CatalogVariant? CatalogVariant { get; set; }
}
