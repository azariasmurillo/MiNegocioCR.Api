using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using System.Numerics;

namespace MiNegocioCR.Api.Infrastructure.Persistence.Repositories
{
    public class WhatsappMessageRepository : IWhatsappMessageRepository
    {
        private readonly AppDbContext _context;

        public WhatsappMessageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task SaveAsync(WhatsAppMessage message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            _context.WhatsAppMessages.Add(message);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(string messageId, MessageStatus status)
        {
            var message = await _context.WhatsAppMessages
                .FirstOrDefaultAsync(x => x.MessageId == messageId);

            if (message == null)
                return;

            message.Status = status;

            await _context.SaveChangesAsync();
        }

        public async Task<List<MessageDto>> GetMessagesAsync(Guid businessId, string phoneNumber, int limit = 50)
        {

            var query = _context.WhatsAppMessages
                .Where(x => x.BusinessId == businessId && x.PhoneNumber == phoneNumber)
                .OrderBy(x => x.Timestamp);

            if (limit > 0)
                query = (IOrderedQueryable<WhatsAppMessage>)query.Take(limit);

            return await query
                .Select(x => new MessageDto
               {
                   PhoneNumber = x.PhoneNumber,
                   Body = x.Body,
                   Timestamp = x.Timestamp,
                   Direction = x.Direction,
                   Status = x.Status,
                   AttachmentUrl = x.AttachmentUrl,
                   AttachmentType = x.AttachmentType
               })
               .ToListAsync();
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(Guid businessId
            ,string? search
            ,string? phone
            ,string? name)
        {
            var query = _context.WhatsAppConversations
                .Where(x => x.BusinessId == businessId && !x.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(x => x.PhoneNumber.Contains(phone));

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.CustomerName != null &&
                                         x.CustomerName.Contains(name));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    x.PhoneNumber.Contains(search) ||
                    (x.CustomerName != null && x.CustomerName.Contains(search)));

            return await query
                .OrderByDescending(x => x.LastMessageAt)
                .Select(x => new ConversationDto
                {
                    PhoneNumber = x.PhoneNumber,
                    CustomerName = x.CustomerName,
                    LastMessage = x.LastMessage,
                    LastMessageAt = x.LastMessageAt,
                    UnreadCount = x.UnreadCount,
                    Status = x.Status,
                    RepairOrderId = x.RepairOrderId
                })
                .ToListAsync();
        }

        public async Task UpdateConversationAsync(Guid businessId,string phone,string messageBody, MessageDirection direction)
        {
            var conversation = await _context.WhatsAppConversations
                .AsTracking()
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.PhoneNumber == phone);

            if (conversation == null)
            {
                conversation = new WhatsAppConversation
                {
                    Id = Guid.NewGuid(),
                    BusinessId = businessId,
                    PhoneNumber = phone,
                    LastMessage = messageBody,
                    LastMessageAt = DateTime.UtcNow,
                    UnreadCount = direction == MessageDirection.Inbound ? 1 : 0
                };
                _context.WhatsAppConversations.Add(conversation);
            }
            else
            {
                conversation.LastMessage = messageBody;
                conversation.LastMessageAt = DateTime.UtcNow;
                if (direction == MessageDirection.Inbound)
                    conversation.UnreadCount += 1;
                else
                    conversation.UnreadCount = 0;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadTotalAsync(Guid businessId)
        {
            return await _context.WhatsAppConversations
                .Where(x => x.BusinessId == businessId && x.UnreadCount > 0)
                .SumAsync(x => x.UnreadCount);
        }

        public async Task ArchiveConversationAsync(Guid conversationId, bool isArchived)
        {
            var conversation = await _context.WhatsAppConversations
                .FirstOrDefaultAsync(x => x.Id == conversationId);

            if (conversation == null)
                throw new Exception("Conversation not found");

            conversation.IsArchived = isArchived;

            await _context.SaveChangesAsync();
        }
    }
}
