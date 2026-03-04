namespace MiNegocioCR.Api.Application.DTOs
{
    public class GetBusinessByIdResultDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool EnableEmailNotifications { get; set; }
        public bool EnableWhatsappNotifications { get; set; }
        public string? WhatsappPhoneNumberId { get; set; }
        public string? WhatsappBusinessAccountId { get; set; }
        public string? WhatsappDisplayPhoneNumber { get; set; }
        public DateTime? WhatsappTokenExpiresAt { get; set; }
        public string? SmtpHost { get; set; }
        public int? SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpFromEmail { get; set; }
        public string? SmtpFromName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? WhatsappAccessToken { get; set; }
    }
}
