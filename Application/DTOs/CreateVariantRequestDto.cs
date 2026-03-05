namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateVariantRequestDto
    {
        public Guid CatalogItemId { get; set; }

        public string SKU { get; set; }

        public decimal Price { get; set; }

        public int InitialStock { get; set; }
    }
}
