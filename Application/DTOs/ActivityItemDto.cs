namespace MiNegocioCR.Api.Application.DTOs;

public class ActivityItemDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
