namespace MiNegocioCR.Api.Domain.Entities
{
    public class WhatsappMessage
    {
        public Guid Id { get; set; }

        public Guid BusinessId { get; set; }

        public string? Phone { get; set; }

        public string? Message { get; set; }

        public string? MetaMessageId { get; set; }        

        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
