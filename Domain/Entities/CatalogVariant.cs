namespace MiNegocioCR.Api.Domain.Entities
{
    public class CatalogVariant
    {
        public Guid Id { get; set; }

        public Guid CatalogItemId { get; set; }

        public string? SKU { get; set; }

        public decimal Price { get; set; }

        /// <summary>Costo unitario de referencia (no altera el precio de venta automáticamente).</summary>
        public decimal CostPrice { get; set; }

        /// <summary>Margen % sobre costo para precios; null = usar <see cref="Business.DefaultProfitMargin"/> del negocio.</summary>
        public decimal? ProfitMargin { get; set; }

        public int StockQuantity { get; set; }

        public int LowStockThreshold { get; set; } = 2;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CatalogItem CatalogItem { get; set; }

        public ICollection<CatalogVariantOptionValue> VariantOptionValues { get; set; } = new List<CatalogVariantOptionValue>();

        public ICollection<RepairOrderItem> RepairOrderItems { get; set; } = new List<RepairOrderItem>();

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

            StockQuantity += quantity;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
            if (StockQuantity < quantity)
                throw new ArgumentException("Not enough stock");

            StockQuantity -= quantity;
        }

        /// <summary>Margen % usado en cálculos de precio: el de la variante o el default del negocio.</summary>
        public decimal ResolveProfitMargin(decimal businessDefaultProfitMargin)
            => ProfitMargin ?? businessDefaultProfitMargin;
    }
}
