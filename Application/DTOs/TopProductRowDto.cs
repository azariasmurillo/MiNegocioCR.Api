namespace MiNegocioCR.Api.Application.DTOs;

public class TopProductRowDto
{
    public string Name { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}
