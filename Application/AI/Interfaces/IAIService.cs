using MiNegocioCR.Api.Application.AI.Models;

namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IAIService
    {
        Task<string> AskAsync(AIRequest request);
    }
}
