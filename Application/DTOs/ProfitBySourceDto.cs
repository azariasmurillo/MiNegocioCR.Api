namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>Ganancia acumulada: reparaciones por <c>RepairOrderId</c>; resto por <c>Source</c>.</summary>
public class ProfitBySourceDto
{
    public decimal Repair { get; set; }
    public decimal Manual { get; set; }
    public decimal Whatsapp { get; set; }
}
