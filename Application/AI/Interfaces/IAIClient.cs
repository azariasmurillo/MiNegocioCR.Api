namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IAIClient
    {
        Task<string> AskAsync(string prompt, string model, int maxTokens);
    }
}
