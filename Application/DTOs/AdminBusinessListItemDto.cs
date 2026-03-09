namespace MiNegocioCR.Api.Application.DTOs;

public class AdminBusinessListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
