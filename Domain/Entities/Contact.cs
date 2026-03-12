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
    }
}
