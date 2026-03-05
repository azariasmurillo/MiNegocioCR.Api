namespace MiNegocioCR.Api.Domain.Entities
{
    public class WhatsappWebhookLog
    {
        public Guid Id { get; set; }
        public string Payload { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
