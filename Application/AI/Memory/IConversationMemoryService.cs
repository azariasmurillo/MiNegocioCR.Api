namespace MiNegocioCR.Api.Application.AI.Memory
{
    public interface IConversationMemoryService
    {
        Task<string> GetConversationContextAsync(Guid businessId,string phoneNumber,int lastMessages = 10);

        Task SaveMessageAsync(
            Guid businessId,
            string phoneNumber,
            string role,
            string message);
    }
}
