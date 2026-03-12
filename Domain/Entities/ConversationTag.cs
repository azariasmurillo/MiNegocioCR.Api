namespace MiNegocioCR.Api.Domain.Entities
{
    public class ConversationTag
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public string Tag { get; set; } = default!;
    }
}
