namespace MiNegocioCR.Api.Domain.Entities
{
    public class Contact
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string Name { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
