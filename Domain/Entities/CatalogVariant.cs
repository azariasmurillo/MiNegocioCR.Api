namespace MiNegocioCR.Api.Domain.Entities
{
    public class CatalogVariant
    {
        public Guid Id { get; set; }

        public Guid CatalogItemId { get; set; }

        public string? SKU { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int LowStockThreshold { get; set; } = 2;

        public bool IsActive { get; set; }

        public CatalogItem CatalogItem { get; set; }
    }
}
