using MiNegocioCR.Api.Application.AI.Models;

namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IAITool
    {
        string Name { get; }

        Task<ToolResult> ExecuteAsync(Guid businessId, string message);
    }
}
