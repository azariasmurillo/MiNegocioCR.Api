namespace MiNegocioCR.Api.Application.DTOs
{
    public class AdjustInventoryRequestDto
    {
        public Guid BusinessId { get; set; }

        public Guid VariantId { get; set; }

        public int Adjustment { get; set; }

        public string Reason { get; set; }
    }
}
