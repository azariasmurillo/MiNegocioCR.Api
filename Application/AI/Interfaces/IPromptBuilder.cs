namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IPromptBuilder
    {
        string BuildPrompt(string businessName, string userMessage);
    }
}
