using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class MessageDto
    {
        public string PhoneNumber { get; set; } = default!;
        public string? Body { get; set; }
        public DateTime Timestamp { get; set; }
        public MessageDirection Direction { get; set; }
        public MessageStatus Status { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
    }
}
