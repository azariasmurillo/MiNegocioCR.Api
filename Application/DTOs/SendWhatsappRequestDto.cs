namespace MiNegocioCR.Api.Application.DTOs
{
    public class SendWhatsappRequestDto
    {
        public Guid BusinessId { get; set; }
        public Guid ConversationId { get; set; }
        public string? Message { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentType { get; set; }
    }
}
