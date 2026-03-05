namespace MiNegocioCR.Api.Domain.Entities
{
    public class CatalogOption
    {
        public Guid Id { get; set; }

        public Guid CatalogItemId { get; set; }

        public string Name { get; set; } = string.Empty;

        public CatalogItem CatalogItem { get; set; } = null!;

        public ICollection<CatalogOptionValue> Values { get; set; } = new List<CatalogOptionValue>();
    }
}
