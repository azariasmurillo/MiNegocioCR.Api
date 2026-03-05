using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Aplication.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;

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

        public async Task<List<WhatsAppMessage>> GetMessagesAsync(Guid businessId, string phoneNumber)
        {
            return await _context.WhatsAppMessages
                .Where(x => x.BusinessId == businessId &&
                            x.PhoneNumber == phoneNumber)
                .OrderBy(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<List<string>> GetConversationsAsync(Guid businessId)
        {
            return await _context.WhatsAppConversations
                .Where(x => x.BusinessId == businessId)
                .Select(x => x.PhoneNumber)
                .Distinct()
                .ToListAsync();
        }

        public async Task UpdateConversationAsync(Guid businessId,string phone,string messageBody)
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
                    UnreadCount = 1
                };
                _context.WhatsAppConversations.Add(conversation);
            }
            else
            {
                conversation.LastMessage = messageBody;
                conversation.LastMessageAt = DateTime.UtcNow;
                conversation.UnreadCount += 1;
            }

            await _context.SaveChangesAsync();
        }
    }
}
