using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Domain.Entities
{
    public class WhatsAppMessage
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public WhatsAppConversation Conversation { get; set; } = default!;

        public string MessageId { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string From { get; set; } = default!;
        public string To { get; set; } = default!;
        public string Body { get; set; } = default!;
        public DateTime Timestamp { get; set; }
        public MessageDirection Direction { get; set; }
        public MessageStatus Status { get; set; }
        public Guid? CustomerId { get; set; }

        public string? MetaMessageId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
    }
}
