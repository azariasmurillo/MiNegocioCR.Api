namespace MiNegocioCR.Api.Domain.Entities
{
    public class Supplier
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
