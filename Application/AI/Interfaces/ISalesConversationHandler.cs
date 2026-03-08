using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface ISalesConversationHandler
    {
        /// <summary>
        /// Maneja los pasos awaiting_confirmation y awaiting_quantity.
        /// </summary>
        /// <returns>Respuesta si se manejó el paso; null si no aplica.</returns>
        Task<string?> HandleAsync(
            Guid businessId,
            string phoneNumber,
            string normalizedMessage,
            ConversationState? conversationState);
    }
}
