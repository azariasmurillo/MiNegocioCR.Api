using MiNegocioCR.Api.Application.AI.Models;

namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IAIChatRequestValidator
    {
        Task<AIChatValidationResult> ValidateAsync(AIRequest request);
    }
}
