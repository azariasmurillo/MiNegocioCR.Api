using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappMessageRepository
    {
        Task SaveAsync(WhatsAppMessage message);
        Task UpdateStatusAsync(string messageId, MessageStatus status);
        Task<List<WhatsAppMessage>> GetMessagesAsync(Guid businessId, string phoneNumber);
        Task<List<string>> GetConversationsAsync(Guid businessId);
        Task UpdateConversationAsync(Guid businessId,string phone,string messageBody);
    }
}
