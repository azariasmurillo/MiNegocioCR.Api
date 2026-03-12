using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities
{
    public class WhatsAppConversation
    {
        public RepairOrder? RepairOrder { get; set; }
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public ConversationStatus Status { get; set; }
        public bool IsArchived { get; set; }
        public Guid? RepairOrderId { get; set; }
        public DateTime CreatedAt { get; set; }        
    }
}
