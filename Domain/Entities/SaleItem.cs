namespace MiNegocioCR.Api.Domain.Entities
{
    public class SaleItem
    {
        public Guid Id { get; set; }

        public Guid SaleId { get; set; }

        public Guid? CatalogVariantId { get; set; }
        public string? Description { get; set; }
        public string ItemType { get; set; } = "Product";

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public Sale Sale { get; set; }
        public decimal UnitPrice { get; internal set; }
        public decimal Total { get; internal set; }
    }
}
