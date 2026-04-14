namespace MiNegocioCR.Api.Application.DTOs
{
    public class MarkConversationReadCommandDto
    {
        public Guid BusinessId { get; set; }
        public Guid ConversationId { get; set; }
    }
}
