using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities
{
    /// <summary>
    /// Agregado raíz del chat WhatsApp (único por BusinessId + PhoneNumber).
    /// </summary>
    public class WhatsAppConversation
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public ConversationStatus Status { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }

        public Business Business { get; set; } = null!;
        public ICollection<WhatsAppMessage> Messages { get; set; } = new List<WhatsAppMessage>();
        public ICollection<ConversationTag> Tags { get; set; } = new List<ConversationTag>();
    }
}
