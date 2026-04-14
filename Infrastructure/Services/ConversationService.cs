using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Domain.Exceptions;

namespace MiNegocioCR.Api.Infrastructure.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IAppDbContext _context;

        public ConversationService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task MarkConversationReadAsync(Guid businessId, Guid conversationId)
        {
            var conversation = await _context.WhatsAppConversations
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.Id == conversationId);

            if (conversation == null)
                return;

            conversation.UnreadCount = 0;
            await _context.SaveChangesAsync(default);
        }

        public async Task CreateConversationAsync(
            Guid businessId,
            string phoneNumber,
            string? customerName)
        {
            var existing = await _context.WhatsAppConversations
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.PhoneNumber == phoneNumber);

            if (existing != null)
                return;

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
            await _context.SaveChangesAsync(default);
        }

        public async Task UpdateStatusAsync(
            Guid businessId,
            string phoneNumber,
            ConversationStatus status)
        {
            var conversation = await _context.WhatsAppConversations
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.PhoneNumber == phoneNumber);

            if (conversation == null)
                throw new NotFoundException("WhatsAppConversation", "Conversation not found");

            conversation.Status = status;
            await _context.SaveChangesAsync(default);
        }
    }
}
