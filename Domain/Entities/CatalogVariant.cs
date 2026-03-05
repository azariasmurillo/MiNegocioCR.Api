namespace MiNegocioCR.Api.Domain.Entities
{
    public class CatalogVariant
    {
        public Guid Id { get; set; }

        public Guid CatalogItemId { get; set; }

        public string? SKU { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        public CatalogItem CatalogItem { get; set; } = null!;
    }
}
