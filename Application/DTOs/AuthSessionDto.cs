namespace MiNegocioCR.Api.Application.DTOs;

/// <summary>
/// Respuesta de login / sesión para el cliente (sin entidades de dominio).
/// </summary>
public class AuthSessionDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>Null si el usuario aún no tiene negocio asignado.</summary>
    public Guid? BusinessId { get; set; }

    public string? BusinessName { get; set; }

    public string Token { get; set; } = string.Empty;
}
