namespace MiNegocioCR.Api.Application.Interfaces.ConversationTag
{
    public interface IConversationTag
    {
        Task AddTagAsync(Guid businessId, Guid conversationId, string tag);
        Task RemoveTagAsync(Guid businessId, Guid conversationId, string tag);
        Task<List<string>> GetTagsAsync(Guid businessId, Guid conversationId);
        /// <summary>Etiquetas distintas usadas en conversaciones del negocio (p. ej. autocompletar filtros).</summary>
        Task<List<string>> GetDistinctTagsForBusinessAsync(Guid businessId);
    }
}
