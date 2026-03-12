namespace MiNegocioCR.Api.Application.DTOs
{
    public class LinkConversationRepairOrderCommandDto
    {
        public Guid BusinessId { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public Guid? RepairOrderId { get; set; }
    }
}
