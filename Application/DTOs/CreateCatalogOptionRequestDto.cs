namespace MiNegocioCR.Api.Application.DTOs
{
    public class BusinessDimensionValueDto
    {
        public Guid Id { get; set; }

        public string DimensionName { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }

    public class CatalogDimensionCatalogDto
    {
        public IReadOnlyList<string> StandardDimensions { get; set; } = Array.Empty<string>();

        public int MaxDimensionsPerProduct { get; set; }
    }

    public class CreateCatalogOptionRequestDto
    {
        public Guid CatalogItemId { get; set; }

        public string? Name { get; set; }

        /// <summary>Si true, permite un nombre fuera del catálogo estándar.</summary>
        public bool IsCustomDimension { get; set; }
    }
}
