using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Application.Interfaces;
using MiNegocioCR.Api.Application.Interfaces.ConversationTag;
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

        public async Task AddTagAsync(Guid conversationId, string tag)
        {
            var entity = new ConversationtagEntity
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                Tag = tag
            };

            _context.ConversationTags.Add(entity);
            CancellationToken cancellationToken = default;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveTagAsync(Guid conversationId, string tag)
        {
            var entity = await _context.ConversationTags
                .FirstOrDefaultAsync(x =>
                    x.ConversationId == conversationId &&
                    x.Tag == tag);

            if (entity != null)
            {
                _context.ConversationTags.Remove(entity);
                CancellationToken cancellationToken = default;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<List<string>> GetTagsAsync(Guid conversationId)
        {
            return await _context.ConversationTags
                .Where(x => x.ConversationId == conversationId)
                .Select(x => x.Tag)
                .ToListAsync();
        }
    }
}
