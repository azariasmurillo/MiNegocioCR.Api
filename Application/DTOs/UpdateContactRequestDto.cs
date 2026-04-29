namespace MiNegocioCR.Api.Application.DTOs;

public class UpdateContactRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
}
