namespace MiNegocioCR.Api.Application.DTOs
{
    public class UpdateVariantRequestDto
    {
        public Guid BusinessId { get; set; }

        public string? SKU { get; set; }

        public decimal Price { get; set; }

        /// <summary>
        /// Si es true, se guarda <see cref="Price"/> tal cual.
        /// Si es false, costo &gt; 0 y margen de variante no nulo tras aplicar <see cref="SetProfitMargin"/>:
        /// precio = costo + ganancia (margen %) + IVA % del negocio.
        /// </summary>
        public bool SetPriceManually { get; set; }

        /// <summary>Costo unitario de referencia (≥ 0).</summary>
        public decimal CostPrice { get; set; }

        /// <summary>Margen % de la variante; solo se persiste cuando SetProfitMargin es true.</summary>
        public decimal? ProfitMargin { get; set; }

        /// <summary>Si es true, se guarda ProfitMargin (null elimina el override y hereda el default del negocio).</summary>
        public bool SetProfitMargin { get; set; }
    }
}
