namespace MiNegocioCR.Api.Domain.Entities;

public class InternetOrderLine
{
    public Guid Id { get; set; }
    public Guid InternetOrderId { get; set; }

    public int SortOrder { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductUrl { get; set; } = string.Empty;
    public decimal UnitPriceUsd { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal LineTotalUsd { get; set; }
    public decimal LineTotalCrc { get; set; }

    public InternetOrder InternetOrder { get; set; } = null!;
}
