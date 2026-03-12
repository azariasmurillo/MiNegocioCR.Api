using Microsoft.AspNetCore.Mvc;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.Interfaces.Whatsapp
{
    public interface IWhatsappMessageRepository
    {
        Task SaveAsync(WhatsAppMessage message);
        Task UpdateStatusAsync(string messageId, MessageStatus status);
        Task<List<MessageDto>> GetMessagesAsync(Guid businessId, string phoneNumber, int limit = 50);
        Task<List<ConversationDto>> GetConversationsAsync(Guid businessId, string? search, 
            string? phone,string? name);
        Task UpdateConversationAsync(Guid businessId,string phone,string messageBody, MessageDirection direction);
        Task<int> GetUnreadTotalAsync(Guid businessId);
    }
}
