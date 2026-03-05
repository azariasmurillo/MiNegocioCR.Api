namespace MiNegocioCR.Api.Domain.Entities
{
    public class CatalogImage
    {
        public Guid Id { get; set; }

        public Guid CatalogItemId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }

        public CatalogItem CatalogItem { get; set; } = null!;
    }
}
