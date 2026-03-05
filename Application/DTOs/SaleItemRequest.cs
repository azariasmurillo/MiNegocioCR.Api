namespace MiNegocioCR.Api.Application.DTOs
{
    public class SaleItemRequestDto
    {
        public Guid VariantId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }
    }
}
