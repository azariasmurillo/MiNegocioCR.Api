namespace MiNegocioCR.Api.Application.DTOs;

public class CreateSaleFromRepairRequestDto
{
    public Guid BusinessId { get; set; }
    public decimal TaxRatePercent { get; set; } = 13m;
    public bool PreventDuplicateInvoiceForRepair { get; set; } = true;
    public List<CreateSaleFromRepairItemDto> Items { get; set; } = new();
}

public class CreateSaleFromRepairItemDto
{
    public string ItemType { get; set; } = "Service"; // Product | Service
    public Guid? CatalogVariantId { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
