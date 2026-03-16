namespace MiNegocioCR.Api.Application.Interfaces.ArchiveConversation
{
    public interface IArchiveConversationUseCase
    {
        Task Execute(Guid conversationId, bool isArchived);
    }
}
