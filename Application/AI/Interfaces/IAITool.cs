namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IAITool
    {
        string Name { get; }

        Task<string> ExecuteAsync(Guid businessId, string query);
    }
}
