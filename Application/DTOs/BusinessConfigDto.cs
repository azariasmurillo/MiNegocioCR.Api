namespace MiNegocioCR.Api.Application.DTOs;

public class BusinessConfigDto
{
    public string? LogoUrl { get; set; }
    public string? BusinessType { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? PublicEmail { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool EnableEmailNotifications { get; set; }
    public bool EnableSsl { get; set; }
}

public class UpdateBusinessConfigRequestDto
{
    public string? BusinessType { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? PublicEmail { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool EnableEmailNotifications { get; set; }
    public bool EnableSsl { get; set; }
}
