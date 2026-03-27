namespace MiNegocioCR.Api.Domain.Entities;

public class Business
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpFromEmail { get; set; }
    public string? SmtpFromName { get; set; }
    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnableWhatsappNotifications { get; set; } = true;
    public string? WhatsappPhoneNumberId { get; set; }
    public string? WhatsappDisplayPhoneNumber { get; set; }
    public string? WhatsappAccessToken { get; set; }
    public string? WhatsappBusinessAccountId { get; set; }
    public DateTime? WhatsappTokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    

    public BusinessSettings? Settings { get; set; }
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
    public ICollection<WhatsAppConversation> WhatsAppConversations { get; set; } = new List<WhatsAppConversation>();
}