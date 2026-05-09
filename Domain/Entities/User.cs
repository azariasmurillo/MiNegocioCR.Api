namespace MiNegocioCR.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    /// <summary>Valores permitidos en BD: Admin, User.</summary>
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;
}
