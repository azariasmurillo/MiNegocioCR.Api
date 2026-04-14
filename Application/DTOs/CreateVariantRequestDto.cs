namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateVariantRequestDto
    {
        public Guid CatalogItemId { get; set; }

        public string SKU { get; set; }

        public decimal Price { get; set; }

        public int InitialStock { get; set; }

        /// <summary>Ids de valores de opción que definen la combinación (ej. color + almacenamiento).</summary>
        public List<Guid> OptionValueIds { get; set; } = new();
    }
}
