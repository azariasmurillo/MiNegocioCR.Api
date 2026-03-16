namespace MiNegocioCR.Api.Application.DTOs
{
    public class CreateConversationCommandDto
    {
        public Guid BusinessId { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string? CustomerName { get; set; }
    }
}
