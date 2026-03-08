namespace MiNegocioCR.Api.Application.AI.Models
{
    public class AIChatValidationResult
    {
        public bool CanContinue { get; init; }
        public string? EarlyResponse { get; init; }
        public string NormalizedMessage { get; init; } = "";

        public static AIChatValidationResult Continue(string normalizedMessage) =>
            new() { CanContinue = true, EarlyResponse = null, NormalizedMessage = normalizedMessage };

        public static AIChatValidationResult EarlyExit(string response, string normalizedMessage = "") =>
            new() { CanContinue = false, EarlyResponse = response, NormalizedMessage = normalizedMessage };
    }
}
