using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class ConversationDto
    {
        public string PhoneNumber { get; set; } = default!;
        public string? CustomerName { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public ConversationStatus Status { get; set; }
        public Guid? RepairOrderId { get; set; }
    }
}
