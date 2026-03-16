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

        public async Task MarkConversationReadAsync(Guid businessId, string phoneNumber)
        {
            var conversation = await _context.WhatsAppConversations
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.PhoneNumber == phoneNumber);

            if (conversation == null)
                return;

            conversation.UnreadCount = 0;
            CancellationToken cancellationToken = default;
            await _context.SaveChangesAsync(cancellationToken);
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
            CancellationToken cancellationToken = default;
            await _context.SaveChangesAsync(cancellationToken);
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
                throw new NotFoundException("Conversation not found");

            conversation.Status = status;
            CancellationToken cancellationToken = default;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task LinkRepairOrderAsync(
            Guid businessId,
            string phoneNumber,
            Guid? repairOrderId)
        {
            var conversation = await _context.WhatsAppConversations
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.PhoneNumber == phoneNumber);

            if (conversation == null)
                throw new NotFoundException("Conversation not found");

            if (repairOrderId != null)
            {
                var repair = await _context.RepairOrders
                    .FirstOrDefaultAsync(x =>
                        x.Id == repairOrderId &&
                        x.BusinessId == businessId);

                if (repair == null)
                    throw new NotFoundException("Repair order not found");
            }

            conversation.RepairOrderId = repairOrderId;
            CancellationToken cancellationToken = default;
            await _context.SaveChangesAsync(cancellationToken);
        }        
    }
}
