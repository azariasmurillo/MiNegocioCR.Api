namespace MiNegocioCR.Api.Domain.Entities;

public class CreditTransactionLine
{
    public Guid Id { get; set; }
    public Guid CreditTransactionId { get; set; }

    public int SortOrder { get; set; }
    public int LineKind { get; set; }
    public Guid? CatalogVariantId { get; set; }
    public string ConceptName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal BaseUnitPriceCrc { get; set; }
    public decimal CreditMarkupPercent { get; set; }
    public decimal UnitPriceCrc { get; set; }
    public decimal LineTotalCrc { get; set; }

    public CreditTransaction CreditTransaction { get; set; } = null!;
    public CatalogVariant? CatalogVariant { get; set; }
}
