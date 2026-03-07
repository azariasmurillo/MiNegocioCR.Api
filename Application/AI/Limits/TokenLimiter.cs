namespace MiNegocioCR.Api.Application.AI.Limits
{
    public class TokenLimiter : ITokenLimiter
    {
        public int GetMaxTokens(string prompt)
        {
            if (prompt.Length < 500)
            {
                return 150;
            }

            return 300;
        }
    }
}
