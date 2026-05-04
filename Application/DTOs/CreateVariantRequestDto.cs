namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateVariantRequestDto
    {
        public Guid CatalogItemId { get; set; }

        public string SKU { get; set; }

        public decimal Price { get; set; }

        /// <summary>
        /// Si es true, se guarda <see cref="Price"/> tal cual.
        /// Si es false y <see cref="CostPrice"/> &gt; 0 y <see cref="ProfitMargin"/> tiene valor,
        /// <c>Price = CostPrice * (1 + ProfitMargin/100)</c>.
        /// </summary>
        public bool SetPriceManually { get; set; }

        /// <summary>Costo unitario de referencia (≥ 0).</summary>
        public decimal CostPrice { get; set; }

        /// <summary>Margen % opcional por SKU; null = al consultar se usa el default del negocio.</summary>
        public decimal? ProfitMargin { get; set; }

        public int InitialStock { get; set; }

        /// <summary>Ids de valores de opción que definen la combinación (ej. color + almacenamiento).</summary>
        public List<Guid> OptionValueIds { get; set; } = new();
    }
}
