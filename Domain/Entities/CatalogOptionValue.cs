namespace MiNegocioCR.Api.Domain.Entities
{
    public class CatalogOptionValue
    {
        public Guid Id { get; set; }

        public Guid CatalogOptionId { get; set; }

        public string Value { get; set; } = string.Empty;

        public CatalogOption CatalogOption { get; set; } = null!;

        public ICollection<CatalogVariantOptionValue> VariantOptionValues { get; set; } = new List<CatalogVariantOptionValue>();
    }
}

