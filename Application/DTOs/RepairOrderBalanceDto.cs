namespace MiNegocioCR.Api.Application.DTOs;

public class RepairOrderBalanceDto
{
    public decimal Subtotal { get; set; }
    /// <summary>Monto de descuento según el porcentaje de la orden.</summary>
    public decimal Discount { get; set; }
    /// <summary>Total de la orden (suma de ítems), antes de considerar pagos.</summary>
    public decimal Total { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
}
