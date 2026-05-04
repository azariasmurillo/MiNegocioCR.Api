namespace MiNegocioCR.Api.Domain.Entities;

public class Business
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? BusinessType { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Location { get; set; }
    public string? PublicEmail { get; set; }
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName { get; set; }
    public bool EnableSsl { get; set; } = true;
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnableWhatsappNotifications { get; set; } = true;
    public string? WhatsappPhoneNumberId { get; set; }
    public string? WhatsappDisplayPhoneNumber { get; set; }
    public string? WhatsappAccessToken { get; set; }
    public string? WhatsappBusinessAccountId { get; set; }
    /// <summary>UTC expiry for user long-lived tokens (from Meta <c>expires_in</c>). Null for System User tokens.</summary>
    public DateTime? WhatsappTokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Porcentaje de ganancia por defecto (referencia para precios; típicamente 0–100). Si la variante no define margen, usar este valor.</summary>
    public decimal DefaultProfitMargin { get; set; }

    public BusinessSettings? Settings { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
    public ICollection<WhatsAppConversation> WhatsAppConversations { get; set; } = new List<WhatsAppConversation>();
}