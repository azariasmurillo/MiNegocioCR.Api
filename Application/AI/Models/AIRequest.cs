namespace MiNegocioCR.Api.Application.AI.Models
{
    public class AIRequest
    {
        public Guid BusinessId { get; set; }

        public string UserMessage { get; set; } = string.Empty;

        public string Channel { get; set; } = "whatsapp";

        public string? PhoneNumber { get; set; }
    }
}
