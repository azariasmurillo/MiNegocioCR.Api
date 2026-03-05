namespace MiNegocioCR.Api.Domain.Entities
{
    public class PurchaseItem
    {
        public Guid Id { get; set; }

        public Guid PurchaseId { get; set; }

        public Guid CatalogVariantId { get; set; }

        public int Quantity { get; set; }

        public decimal Cost { get; set; }

        public Purchase Purchase { get; set; } = null!;
    }
}
