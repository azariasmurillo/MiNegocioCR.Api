namespace MiNegocioCR.Api.Domain.Entities
{
    public class ConversationState
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public Guid? ProductId { get; set; }

        public decimal? Price { get; set; }

        public string Step { get; set; } = null!;

        public DateTime UpdatedAt { get; set; }
    }
}
