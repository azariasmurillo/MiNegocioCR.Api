using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities
{
    public class InventoryMovement
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public Guid CatalogVariantId { get; set; }

        public InventoryMovementType Type { get; set; }

        public int Quantity { get; set; }

        public string? Reference { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CatalogVariant Variant { get; set; }
    }
}
