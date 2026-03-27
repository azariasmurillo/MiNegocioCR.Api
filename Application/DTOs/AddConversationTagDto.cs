namespace MiNegocioCR.Api.Application.DTOs
{
    public class AddConversationTagDto
    {
        /// <summary>Negocio propietario (validación multitenant).</summary>
        public Guid BusinessId { get; set; }
        public Guid ConversationId { get; set; }
        public string Tag { get; set; } = default!;
    }
}
