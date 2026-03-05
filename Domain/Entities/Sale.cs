namespace MiNegocioCR.Api.Domain.Entities
{
    public class Sale
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public DateTime SaleDate { get; set; }

        public decimal Total { get; set; }

        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
