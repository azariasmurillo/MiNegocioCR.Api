using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class ConversationDto
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string? CustomerName { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public ConversationStatus Status { get; set; }
        /// <summary>Etiquetas asociadas a la conversación (p. ej. para filtros en UI).</summary>
        public List<string> Tags { get; set; } = new();
    }
}
