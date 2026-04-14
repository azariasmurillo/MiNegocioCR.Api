using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IConversationService
    {
        Task MarkConversationReadAsync(Guid businessId, Guid conversationId);
        Task CreateConversationAsync(Guid businessId, string phoneNumber, string? customerName);
        Task UpdateStatusAsync(Guid businessId, string phoneNumber, ConversationStatus status);
    }
}
