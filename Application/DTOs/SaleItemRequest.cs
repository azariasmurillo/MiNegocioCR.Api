namespace MiNegocioCR.Api.Application.DTOs
{
    public class SaleItemRequestDto
    {
        public Guid? CatalogVariantId { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ItemType { get; set; } = "Product"; // Product | Service
    }
}
