using Microsoft.EntityFrameworkCore;
using MiNegocioCR.Api.Domain.Entities;
using MiNegocioCR.Api.Infrastructure.Persistence;

namespace MiNegocioCR.Api.Application.AI.State
{
    public class ConversationStateService : IConversationStateService
    {
        private readonly AppDbContext _context;

        public ConversationStateService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationState?> GetAsync(Guid businessId, string phoneNumber)
        {
            return await _context.ConversationStates
                .FirstOrDefaultAsync(x =>
                    x.BusinessId == businessId &&
                    x.PhoneNumber == phoneNumber);
        }

        public async Task SaveAsync(ConversationState state)
        {
            var existing = await GetAsync(state.BusinessId, state.PhoneNumber);

            if (existing == null)
            {
                _context.ConversationStates.Add(state);
            }
            else
            {
                existing.ProductId = state.ProductId;
                existing.Price = state.Price;
                existing.Step = state.Step;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task ClearAsync(Guid businessId, string phoneNumber)
        {
            var state = await GetAsync(businessId, phoneNumber);

            if (state != null)
            {
                _context.ConversationStates.Remove(state);
                await _context.SaveChangesAsync();
            }
        }
    }
}
