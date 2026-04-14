using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.ConversationTag;
using MiNegocioCR.Api.Domain.Exceptions;
using ConversationtagEntity = MiNegocioCR.Api.Domain.Entities.ConversationTag;

namespace MiNegocioCR.Api.Application.ConversationTag
{
    public class ConversationTag : IConversationTag
    {
        private readonly IAppDbContext _context;

        public ConversationTag(IAppDbContext context)
        {
            _context = context;
        }

        public async Task AddTagAsync(Guid businessId, Guid conversationId, string tag)
        {
            var normalized = NormalizeTag(tag);
            if (string.IsNullOrEmpty(normalized))
                throw new ArgumentException("Tag is required.", nameof(tag));

            var conv = await _context.WhatsAppConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == conversationId && x.BusinessId == businessId);
            if (conv == null)
                throw new NotFoundException("WhatsAppConversation", "Conversation not found");

            var exists = await _context.ConversationTags
                .AnyAsync(x => x.ConversationId == conversationId && x.Tag == normalized);
            if (exists)
                return;

            var entity = new ConversationtagEntity
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Tag = normalized
            };

            _context.ConversationTags.Add(entity);
            await _context.SaveChangesAsync(default);
        }

        public async Task RemoveTagAsync(Guid businessId, Guid conversationId, string tag)
        {
            var normalized = NormalizeTag(tag);
            if (string.IsNullOrEmpty(normalized))
                return;

            var conv = await _context.WhatsAppConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == conversationId && x.BusinessId == businessId);
            if (conv == null)
                throw new NotFoundException("WhatsAppConversation", "Conversation not found");

            var entity = await _context.ConversationTags
                .FirstOrDefaultAsync(x =>
                    x.ConversationId == conversationId &&
                    x.Tag == normalized);

            if (entity != null)
            {
                _context.ConversationTags.Remove(entity);
                await _context.SaveChangesAsync(default);
            }
        }

        public async Task<List<string>> GetTagsAsync(Guid businessId, Guid conversationId)
        {
            var conv = await _context.WhatsAppConversations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == conversationId && x.BusinessId == businessId);
            if (conv == null)
                throw new NotFoundException("WhatsAppConversation", "Conversation not found");

            return await _context.ConversationTags
                .Where(x => x.ConversationId == conversationId)
                .OrderBy(x => x.Tag)
                .Select(x => x.Tag)
                .ToListAsync();
        }

        public async Task<List<string>> GetDistinctTagsForBusinessAsync(Guid businessId)
        {
            return await _context.ConversationTags
                .Where(t => t.Conversation.BusinessId == businessId)
                .Select(t => t.Tag)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        private static string NormalizeTag(string tag) => tag.Trim();
    }
}
