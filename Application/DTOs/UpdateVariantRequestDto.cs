namespace MiNegocioCR.Api.Application.DTOs
{
    public class UpdateVariantRequestDto
    {
        public Guid BusinessId { get; set; }

        public string? SKU { get; set; }

        public decimal Price { get; set; }
    }
}
