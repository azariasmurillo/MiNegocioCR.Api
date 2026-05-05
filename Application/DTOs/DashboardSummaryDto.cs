namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>Resumen accionable del día (UTC) para el dashboard.</summary>
public class DashboardSummaryDto
{
    public decimal IngresosHoy { get; set; }
    public decimal GananciaHoy { get; set; }
    public decimal TicketPromedio { get; set; }
    public int OrdenesActivas { get; set; }
}
