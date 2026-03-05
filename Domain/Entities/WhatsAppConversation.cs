namespace MiNegocioCR.Api.Domain.Entities
{
    public class WhatsAppConversation
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string LastMessage { get; set; } = default!;
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
