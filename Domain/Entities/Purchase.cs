namespace MiNegocioCR.Api.Domain.Entities
{
    public class Purchase
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public Guid SupplierId { get; set; }

        public DateTime PurchaseDate { get; set; }

        public decimal Total { get; set; }

        public Supplier Supplier { get; set; } = null!;

        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}
