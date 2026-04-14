namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappApplicationService
    {
        /// <summary>Envío por conversación (API principal).</summary>
        Task SendByConversationIdAsync(Guid businessId, Guid conversationId, string message,
            string? attachmentUrl = null, string? attachmentType = null);

        /// <summary>Compatibilidad: resuelve conversación por teléfono (IA, plantillas).</summary>
        Task SendAsync(Guid businessId, string phone, string message, string? attachmentUrl = null,
            string? attachmentType = null);

        Task ConnectAsync(Guid businessId, string phoneNumberId, string accessToken,
            CancellationToken cancellationToken = default);
    }
}
