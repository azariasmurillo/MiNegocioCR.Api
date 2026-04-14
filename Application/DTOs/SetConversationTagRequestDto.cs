namespace MiNegocioCR.Api.Application.DTOs
{
    /// <summary>
    /// Alta de etiqueta en una conversación (usa <see cref="BusinessId"/> para validar pertenencia).
    /// </summary>
    public class SetConversationTagRequestDto
    {
        public Guid BusinessId { get; set; }
        public string Tag { get; set; } = default!;
    }
}
