namespace MiNegocioCR.Api.Application.DTOs;

public class SalesListQueryDto
{
    /// <summary>Inicio del rango (UTC): 00:00 del día en Costa Rica.</summary>
    public DateTime? From { get; set; }

    /// <summary>Fin exclusivo del rango (UTC): 00:00 del día siguiente al último día en CR.</summary>
    public DateTime? ToExclusive { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Sort { get; set; } = "createdAt desc";
    public string? PaymentMethod { get; set; }

    /// <summary>Si true, solo ventas ligadas a una orden (<c>RepairOrderId</c>).</summary>
    public bool? FromRepair { get; set; }
}
