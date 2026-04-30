namespace MiNegocioCR.Api.Application.DTOs;

public class DashboardSummaryDto
{
    public int SalesTodayCount { get; set; }
    public decimal SalesTodayTotal { get; set; }
    public int OrdersPendingCount { get; set; }
    public int OrdersInProcessCount { get; set; }
    public int OrdersProcessedCount { get; set; }
    public int OrdersDeliveredCount { get; set; }
    public int InvoicesTodayCount { get; set; }
}
