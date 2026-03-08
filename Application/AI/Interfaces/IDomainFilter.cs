namespace MiNegocioCR.Api.Application.AI.Interfaces
{
    public interface IDomainFilter
    {
        bool IsAllowed(string message);
    }
}
