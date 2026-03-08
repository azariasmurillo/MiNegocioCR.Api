using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IToolSelector
    {
        IAITool Select(AIIntent intent);
    }
}
