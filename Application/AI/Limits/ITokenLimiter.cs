namespace MiNegocioCR.Api.Application.AI.Limits
{
    public interface ITokenLimiter
    {
        int GetMaxTokens(string prompt);
    }
}
