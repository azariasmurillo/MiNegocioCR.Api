namespace MiNegocioCR.Api.Domain.Entities
{
    /// <summary>
    /// Une una variante del catálogo con un valor de opción (combinación, ej. Color=Negro + Tamaño=16GB).
    /// </summary>
    public class CatalogVariantOptionValue
    {
        public Guid Id { get; set; }

        public Guid CatalogVariantId { get; set; }

        public Guid CatalogOptionValueId { get; set; }

        public CatalogVariant CatalogVariant { get; set; } = null!;

        public CatalogOptionValue CatalogOptionValue { get; set; } = null!;
    }
}
