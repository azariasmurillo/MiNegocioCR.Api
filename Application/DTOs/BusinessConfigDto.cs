namespace MiNegocioCR.Api.Application.DTOs;

public class BusinessConfigDto
{
    public string? LogoUrl { get; set; }
    public string? BusinessType { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? PublicEmail { get; set; }
}

public class UpdateBusinessConfigRequestDto
{
    public string? BusinessType { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? PublicEmail { get; set; }
}
