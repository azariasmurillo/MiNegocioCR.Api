namespace MiNegocioCR.Api.Application.DTOs;

public class ContactResponseDto
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
