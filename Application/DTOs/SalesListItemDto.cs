namespace MiNegocioCR.Api.Application.DTOs;

public class SalesListItemDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public decimal Total { get; set; }
    public decimal TotalOrden { get; set; }
    public decimal PrepaidAmount { get; set; }
    public List<SalePaymentMethodDto> PaymentMethods { get; set; } = new();
}
