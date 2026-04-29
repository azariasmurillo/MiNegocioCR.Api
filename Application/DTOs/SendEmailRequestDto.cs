namespace MiNegocioCR.Api.Application.DTOs;

public class SendEmailRequestDto
{
    public string? Email { get; set; }
    public string HtmlContent { get; set; } = string.Empty;
}
