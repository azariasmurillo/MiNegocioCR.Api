using MiNegocioCR.Api.Domain.Entities;

namespace MiNegocioCR.Api.Application.AI.State
{
    public interface IConversationStateService
    {
        Task<ConversationState?> GetAsync(Guid businessId, string phoneNumber);

        Task SaveAsync(ConversationState state);

        Task ClearAsync(Guid businessId, string phoneNumber);
    }
}
