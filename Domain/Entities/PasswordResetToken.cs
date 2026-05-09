namespace MiNegocioCR.Api.Domain.Entities;

/// <summary>
/// Historial de solicitudes de recuperación. <see cref="Token"/> almacena hash SHA-256 (hex) del token enviado por email.
/// </summary>
public class PasswordResetToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
