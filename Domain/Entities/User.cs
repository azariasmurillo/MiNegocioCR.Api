namespace MiNegocioCR.Api.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public Guid BusinessId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Business Business { get; set; } = null!;
}