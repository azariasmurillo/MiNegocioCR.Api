namespace MiNegocioCR.Api.Application.AI.Limits
{
    public interface IAITokenBudgetService
    {
        Task<bool> CanUseAsync(Guid businessId, int tokens);
    }
}
