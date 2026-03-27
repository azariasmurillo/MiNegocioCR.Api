using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces.Whatsapp;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Domain.Enums;
using MiNegocioCR.Api.Infrastructure.Persistence;
using System.Text;

namespace MiNegocioCR.Api.Application.AI.Memory
{
    public class ConversationMemoryService : IConversationMemoryService
    {
        private readonly AppDbContext _context;
        private readonly IWhatsappMessageRepository _whatsappMessageRepository;

        public ConversationMemoryService(
            AppDbContext context,
            IWhatsappMessageRepository whatsappMessageRepository)
        {
            _context = context;
            _whatsappMessageRepository = whatsappMessageRepository;
        }

        public async Task<string> GetConversationContextAsync(Guid businessId, string phoneNumber, int lastMessages = 10)
        {
            var messages = await _context.WhatsAppMessages
                .Where(x =>
                    x.PhoneNumber == phoneNumber &&
                    x.Conversation.BusinessId == businessId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(lastMessages)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();

            var sb = new StringBuilder();

            foreach (var msg in messages)
            {
                var role = msg.Direction == MessageDirection.Inbound ? "user" : "assistant";
                sb.AppendLine($"{role}: {msg.Body}");
            }

            return sb.ToString();
        }

        public async Task SaveMessageAsync(
            Guid businessId,
            string phoneNumber,
            string role,
            string message)
        {
            var direction = role == "user"
                ? MessageDirection.Inbound
                : MessageDirection.Outbound;

            var conversation = await _whatsappMessageRepository.GetOrCreateConversationAsync(businessId, phoneNumber, null);

            var msg = new WhatsAppMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                MessageId = $"ai-{Guid.NewGuid():N}",
                PhoneNumber = phoneNumber,
                From = direction == MessageDirection.Inbound ? phoneNumber : "ai",
                To = direction == MessageDirection.Inbound ? "ai" : phoneNumber,
                Body = message,
                Timestamp = DateTime.UtcNow,
                Direction = direction,
                Status = MessageStatus.Sent,
                CreatedAt = DateTime.UtcNow
            };

            _context.WhatsAppMessages.Add(msg);
            await _context.SaveChangesAsync();
        }
    }
}
