namespace MiNegocioCR.Api.Domain.Entities
{
    public class Sale
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public Guid? ContactId { get; set; }
        public Contact? Contact { get; set; }

        public DateTime SaleDate { get; set; }

        public decimal Total { get; set; }

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
        public DateTime CreatedAt { get; internal set; }
        public string CustomerPhone { get; internal set; } = string.Empty;
        public decimal TotalAmount { get; internal set; }
    }
}
