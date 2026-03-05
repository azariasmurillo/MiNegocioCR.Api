namespace MiNegocioCR.Api.Domain.Entities
{
    public class CatalogCategory
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CatalogItem> Items { get; set; } = new List<CatalogItem>();
    }
}
