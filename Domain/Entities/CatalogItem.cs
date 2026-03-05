using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities
{
    public class CatalogItem
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public Guid? CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public CatalogItemType Type { get; set; }

        public bool HasVariants { get; set; }

        public decimal BasePrice { get; set; }

        public bool TrackStock { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CatalogCategory? Category { get; set; }

        public ICollection<CatalogVariant> Variants { get; set; } = new List<CatalogVariant>();

        public ICollection<CatalogImage> Images { get; set; } = new List<CatalogImage>();
    }
}
