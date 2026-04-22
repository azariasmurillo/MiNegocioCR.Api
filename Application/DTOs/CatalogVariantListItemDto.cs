namespace MiNegocioCR.Api.Application.DTOs
{
    public class CatalogVariantListItemDto
    {
        public Guid Id { get; set; }

        public Guid CatalogItemId { get; set; }

        public string CatalogItemName { get; set; } = string.Empty;

        public string? Sku { get; set; }

        public decimal Price { get; set; }

        /// <summary>Cantidad del movimiento de stock inicial al crear la variante; 0 si no hubo movimiento.</summary>
        public int InitialStock { get; set; }

        public int CurrentStock { get; set; }

        public List<Guid> OptionValueIds { get; set; } = new();

        /// <summary>Ej. "Capacidad: 256GB" (NombreOpción: Valor).</summary>
        public List<string> OptionValueLabels { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
