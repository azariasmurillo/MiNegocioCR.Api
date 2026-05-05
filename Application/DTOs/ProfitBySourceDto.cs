namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>Ganancia acumulada por <c>Sale.Source</c> (Repair, Manual, WhatsApp).</summary>
public class ProfitBySourceDto
{
    public decimal Repair { get; set; }
    public decimal Manual { get; set; }
    public decimal Whatsapp { get; set; }
}
