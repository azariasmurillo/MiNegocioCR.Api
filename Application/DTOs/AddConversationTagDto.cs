namespace MiNegocioCR.Api.Application.DTOs
{
    public class AddConversationTagDto
    {
        public Guid ConversationId { get; set; }
        public string Tag { get; set; } = default!;
    }
}
