using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappMessageRepository
    {
        Task SaveAsync(WhatsAppMessage message);
        Task UpdateStatusAsync(string messageId, MessageStatus status);
        Task<List<MessageDto>> GetMessagesAsync(Guid businessId, Guid conversationId, int limit = 50);
        Task<List<ConversationDto>> GetConversationsAsync(Guid businessId, string? search,
            string? phone, string? name, string? tag = null);
        Task UpdateConversationAfterMessageAsync(Guid conversationId, string messageBody, MessageDirection direction);
        Task<int> GetUnreadTotalAsync(Guid businessId);
        Task ArchiveConversationAsync(Guid conversationId, bool isArchived);
        Task<WhatsAppConversation?> GetConversationByIdAsync(Guid conversationId, Guid businessId);
        Task<WhatsAppConversation> GetOrCreateConversationAsync(Guid businessId, string phoneNumber, string? customerName = null);
        Task<bool> MessageExistsByMetaIdAsync(string messageId);
    }
}
