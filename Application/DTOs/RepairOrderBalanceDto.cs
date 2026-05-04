namespace MiNegocioCR.Api.Application.DTOs;

public class RepairOrderBalanceDto
{
    public decimal TotalOrden { get; set; }
    public decimal TotalPagado { get; set; }
    public decimal SaldoPendiente { get; set; }
}
