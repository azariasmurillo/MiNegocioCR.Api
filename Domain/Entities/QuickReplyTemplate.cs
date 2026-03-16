namespace MiNegocioCR.Api.Domain.Entities
{
    public class QuickReplyTemplate
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public string Name { get; set; } = default!;
        public string MessageText { get; set; } = default!;
    }
}
