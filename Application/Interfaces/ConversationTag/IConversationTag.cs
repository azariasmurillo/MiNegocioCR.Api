namespace MiNegocioCR.Api.Application.Interfaces.ConversationTag
{
    public interface IConversationTag
    {
        Task AddTagAsync(Guid conversationId, string tag);
        Task RemoveTagAsync(Guid conversationId, string tag);
        Task<List<string>> GetTagsAsync(Guid conversationId);
    }
}
