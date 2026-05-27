namespace MiNegocioCR.Api.Application.DTOs;

public class SalesListItemDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public Guid? RepairOrderId { get; set; }
    public string Source { get; set; } = "Manual";
    public decimal TaxAmount { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal Total { get; set; }
    public decimal TotalOrden { get; set; }
    public decimal PrepaidAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string DiscountKind { get; set; } = "None";
    public decimal DiscountInputValue { get; set; }
    public List<SalePaymentMethodDto> PaymentMethods { get; set; } = new();
}
