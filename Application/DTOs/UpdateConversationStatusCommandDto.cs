using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.DTOs
{
    public class UpdateConversationStatusCommandDto
    {
        public Guid BusinessId { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public ConversationStatus Status { get; set; }
    }
}
