namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>Respuesta de lookup por SKU (app móvil / escáner barcode).</summary>
public sealed class VariantBySkuLookupDto
{
    public Guid VariantId { get; init; }

    public Guid CatalogItemId { get; init; }

    public string CatalogItemName { get; init; } = string.Empty;

    /// <summary>Etiqueta de presentación (valores de dimensión unidos) o vacío si producto simple.</summary>
    public string? PresentationLabel { get; init; }

    public string? Sku { get; init; }

    public int CurrentStock { get; init; }

    public decimal Price { get; init; }

    public decimal CostPrice { get; init; }

    public decimal? ProfitMargin { get; init; }

    public decimal EffectiveProfitMargin { get; init; }

    public string? PrimaryImageUrl { get; init; }

    public int ImageCount { get; init; }

    public bool IsActive { get; init; } = true;

    public List<string> OptionValueLabels { get; init; } = new();
}
