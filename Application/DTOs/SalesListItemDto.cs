namespace MiNegocioCR.Api.Application.DTOs;

public class SalesListItemDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public decimal Total { get; set; }
    public bool PayCash { get; set; }
    public bool PayTransfer { get; set; }
    public bool PaySinpe { get; set; }
    public bool PayCard { get; set; }
}
