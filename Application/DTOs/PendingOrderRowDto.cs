namespace MiNegocioCR.Api.Application.DTOs;

public class PendingOrderRowDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal PendingAmount { get; set; }
}
