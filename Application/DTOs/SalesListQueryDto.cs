namespace MiNegocioCR.Api.Application.DTOs;

public class SalesListQueryDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Sort { get; set; } = "createdAt desc";
    public string? PaymentMethod { get; set; }
}
