namespace MiNegocioCR.Api.Application.DTOs
{
    public class RegisterPurchaseRequestDto
    {
        public Guid BusinessId { get; set; }

        public Guid VariantId { get; set; }

        public int Quantity { get; set; }

        public decimal Cost { get; set; }
    }
}
