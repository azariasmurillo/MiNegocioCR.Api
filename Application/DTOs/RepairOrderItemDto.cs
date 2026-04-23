namespace MiNegocioCR.Api.Application.DTOs;

public class RepairOrderItemDto
{
    public Guid? CatalogVariantId { get; set; }

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}
