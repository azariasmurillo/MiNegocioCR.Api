using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.DTOs;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

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

        public async Task<List<MessageDto>> GetMessagesAsync(Guid businessId, Guid conversationId, int limit = 50)
        {
            var baseQuery = _context.WhatsAppMessages
                .Where(x =>
                    x.ConversationId == conversationId &&
                    x.Conversation.BusinessId == businessId);

            if (limit <= 0)
            {
                return await baseQuery
                    .OrderBy(x => x.Timestamp)
                    .Select(x => new MessageDto
                    {
                        Id = x.Id,
                        ConversationId = x.ConversationId,
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

            var last = await baseQuery
                .OrderByDescending(x => x.Timestamp)
                .Take(limit)
                .Select(x => new MessageDto
                {
                    Id = x.Id,
                    ConversationId = x.ConversationId,
                    PhoneNumber = x.PhoneNumber,
                    Body = x.Body,
                    Timestamp = x.Timestamp,
                    Direction = x.Direction,
                    Status = x.Status,
                    AttachmentUrl = x.AttachmentUrl,
                    AttachmentType = x.AttachmentType
                })
                .ToListAsync();

            return last.OrderBy(x => x.Timestamp).ToList();
        }

        public async Task<List<ConversationDto>> GetConversationsAsync(Guid businessId
            , string? search
            , string? phone
            , string? name
            , string? tag = null)
        {
            var query = _context.WhatsAppConversations
                .AsNoTracking()
                .Where(x => x.BusinessId == businessId && !x.IsArchived)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(phone))
                query = query.Where(x => x.PhoneNumber != null && x.PhoneNumber.Contains(phone));

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.CustomerName != null &&
                                         x.CustomerName.Contains(name));

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(x =>
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(search)) ||
                    (x.CustomerName != null && x.CustomerName.Contains(search)));

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var t = tag.Trim();
                query = query.Where(c => c.Tags.Any(x => x.Tag == t));
            }

            var rows = await query
                .OrderByDescending(x => x.LastMessageAt)
                .Include(x => x.Tags)
                .AsSplitQuery()
                .ToListAsync();

            return rows.ConvertAll(x => new ConversationDto
            {
                Id = x.Id,
                PhoneNumber = x.PhoneNumber ?? "",
                CustomerName = x.CustomerName,
                LastMessage = x.LastMessage,
                LastMessageAt = x.LastMessageAt,
                UnreadCount = x.UnreadCount,
                Status = x.Status,
                Tags = x.Tags.Select(t => t.Tag).OrderBy(s => s).ToList()
            });
        }

        public async Task UpdateConversationAfterMessageAsync(Guid conversationId, string messageBody,
            MessageDirection direction)
        {
            var conversation = await _context.WhatsAppConversations
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == conversationId);

            if (conversation == null)
                throw new NotFoundException("WhatsAppConversation", "Conversation not found");

            conversation.LastMessage = messageBody;
            conversation.LastMessageAt = DateTime.UtcNow;
            if (direction == MessageDirection.Inbound)
                conversation.UnreadCount += 1;
            else
                conversation.UnreadCount = 0;

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
                throw new NotFoundException("WhatsAppConversation", "Conversation not found");

            conversation.IsArchived = isArchived;

            await _context.SaveChangesAsync();
        }

        public async Task<WhatsAppConversation?> GetConversationByIdAsync(Guid conversationId, Guid businessId)
        {
            return await _context.WhatsAppConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == conversationId && x.BusinessId == businessId);
        }

        public async Task<WhatsAppConversation> GetOrCreateConversationAsync(Guid businessId, string phoneNumber,
            string? customerName = null)
        {
            var existing = await _context.WhatsAppConversations
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.PhoneNumber == phoneNumber);

            if (existing != null)
                return existing;

            var conversation = new WhatsAppConversation
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                PhoneNumber = phoneNumber,
                CustomerName = customerName,
                Status = ConversationStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.WhatsAppConversations.Add(conversation);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task<bool> MessageExistsByMetaIdAsync(string messageId)
        {
            return await _context.WhatsAppMessages
                .AnyAsync(x => x.MessageId == messageId);
        }
    }
}
